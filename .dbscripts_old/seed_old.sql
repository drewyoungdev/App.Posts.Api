\c posts_db
--### Create posts table
CREATE TABLE IF NOT EXISTS posts (
  id          serial primary key,
  parent_id   integer,
  upvotes     integer,
  downvotes   integer,
  author      varchar(20),
  create_date timestamp,
  body     text
);
--### Insert dev records
INSERT INTO posts (id, parent_id, upvotes, downvotes, author, create_date, body) VALUES 
('1', NULL, '10', '5', 'user1', '2010-08-20T15:00:00', 'This is a root-post #1'),
('2', '1', '5500', '4500', 'user1', '2010-08-20T15:00:00', 'Ranked lower with higher (upvotes - downvotes) (55% positive)'),
('3', '1', '600', '400', 'user1', '2010-08-20T15:00:00', 'Ranked higher with lower (upvotes - downvotes) (60% positive)'),
('4', NULL, '10', '5', 'user1', '2010-08-20T15:00:00', 'This is a root-post #2'),
('5', '4', '2', '0', 'user1', '2010-08-20T15:00:00', 'Ranked lower with higher average rating (100% positive)'),
('6', '4', '100', '1', 'user1', '2010-08-20T15:00:00', 'Ranked higher with lower average rating (99% positive)');
-- (NULL, '0', 'user1', '2010-08-20T15:00:00', 'This is a low-tier comment'),
-- (NULL, '30', 'user1', '2010-08-20T15:00:00', 'This is a high-tier comment'),
-- ('1', '5', 'user1', '2010-08-20T15:00:00', 'Reply to "1" mid-tier comment'),
-- ('1', '12', 'user1', '2010-08-20T15:00:00', 'Reply to "1" high-tier comment'),
-- ('1', '0', 'user1', '2010-08-20T15:00:00', 'Reply to "1" low-tier comment'),
-- ('5', '12', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
-- ('5', '12', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
-- ('5', '0', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
-- ('5', '5', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
-- ('5', '5', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
-- ('8', '0', 'user1', '2010-08-20T15:00:00', 'Reply to "8" comment'),
-- ('8', '2', 'user1', '2010-08-20T15:00:00', 'Reply to "8" comment'),
-- ('8', '3', 'user1', '2010-08-20T15:00:00', 'Reply to "8" comment'),
-- ('5', '2', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
-- (NULL, '3', 'user1', '2010-08-20T15:00:00', 'Reply to "8" comment'),
-- ('16', '0', 'user1', '2010-08-20T15:00:00', 'Level 1'),
-- ('17', '0', 'user1', '2010-08-20T15:00:00', 'Level 2'),
-- ('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 3'),
-- ('19', '0', 'user1', '2010-08-20T15:00:00', 'Level 4'),
-- ('20', '0', 'user1', '2010-08-20T15:00:00', 'Level 5'),
-- ('21', '0', 'user1', '2010-08-20T15:00:00', 'Level 6'),
-- ('22', '0', 'user1', '2010-08-20T15:00:00', 'Level 7'),
-- ('17', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.1'),
-- ('21', '0', 'user1', '2010-08-20T15:00:00', 'Level 6.1'),
-- ('21', '0', 'user1', '2010-08-20T15:00:00', 'Level 6.2'),
-- ('19', '0', 'user1', '2010-08-20T15:00:00', 'Level 3.1'),
-- ('19', '0', 'user1', '2010-08-20T15:00:00', 'Level 3.2'),
-- ('19', '0', 'user1', '2010-08-20T15:00:00', 'Level 3.3'),
-- ('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.1'),
-- ('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.2'),
-- ('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.3'),
-- ('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.4');
--### Create function to get post tree
CREATE FUNCTION get_replies_by_parent_id(input_parent_id INT, direct_reply_limit INT, depth_limit INT, recursive_limit INT)
RETURNS TABLE(id INT, parent_id INT, author VARCHAR, create_date TIMESTAMP, score INT, comment TEXT, depth INT, num_of_replies BIGINT)
AS 
$$
BEGIN
	RETURN QUERY
	
	WITH RECURSIVE posts_tree AS (
	  -- First select performed to get top level rows
	  (SELECT
		 p.id,
		 p.parent_id,
		 p.author,
		 p.create_date,
		 p.score,
		 p.comment,
		 0 depth,
		 -- vv An upvote and downvote should be sent to Lower Wilson Bound algorithm here instead vv
		 ARRAY[-p.score, p.id] path   -- used to sort by vote then ID
	  FROM posts p
		-- Filter for all generations of children of parent (e.g. get replies to root post)
		WHERE p.parent_id = input_parent_id
		-- this should then order by the result of the Lower Wilson Bound algorithm
		ORDER BY p.score DESC, p.id ASC
		LIMIT direct_reply_limit
	  )
	  -- test UNION ALL for performance
	  UNION ALL
	  -- Self referential select performed repeatedly until no more rows are found
	  (SELECT
		 p.id,
		 p.parent_id,
		 p.author,
		 p.create_date,
		 p.score,
		 p.comment,
		 pt.depth + 1,
		 pt.path || ARRAY[-p.score, p.id]
	  FROM posts p
		JOIN posts_tree pt ON p.parent_id = pt.id
	    --This limits the number of recursive joins made at each depth and limits the depth.
		--Reddit displays their posts based on what they think is most valuable in the conversation.
		--However, if no activity on thread, then these rules should not apply.
		--Only at certain points does the algorithm kick in
		WHERE pt.depth + 1 <= depth_limit
		-- this should then order by the result of the Lower Wilson Bound algorithm
		ORDER BY p.score DESC, p.id ASC
		-- only show comments that meet a certain threshold
	   	LIMIT recursive_limit
	  )
	)
	
	SELECT 
	ptree.id,
	ptree.parent_id,
	ptree.author,
	ptree.create_date,
	ptree.score, 
	ptree.comment,
	ptree.depth,
	-- Gather count of replies (Query to find out how many other posts reference current one as parent_id)
	(SELECT COUNT(p.id) FROM posts p WHERE p.parent_id = ptree.id) AS num_of_replies
	FROM posts_tree ptree
	-- Removed since new limit parameters will prevent any unnecessary recursive calculations
	ORDER BY ptree.path;

END;
$$ 
LANGUAGE 'plpgsql';
-- ### Create function to get a root post + it's replies
CREATE FUNCTION get_root_post_with_replies(input_id INT)
RETURNS TABLE(id INT, parent_id INT, author VARCHAR, create_date TIMESTAMP, score INT, comment TEXT, depth INT, num_of_replies BIGINT)
AS 
$$ 
DECLARE
	parent_num_of_replies integer;
	direct_reply_limit INT;
	depth_limit INT;
	recursive_limit INT;
BEGIN

	-- Retrieve root post data. Count will dictate how many nested replies we will retrieve
	CREATE TEMP TABLE root_post AS
	SELECT 	
		p.id,
		p.parent_id,
		p.author,
		p.create_date,
		p.score, 
		p.comment,
		0 depth,
		(SELECT COUNT(sub_p.id) FROM posts sub_p WHERE sub_p.parent_id = p.id) AS num_of_replies
		FROM posts p where p.id = input_id;

	SELECT rp.num_of_replies INTO parent_num_of_replies FROM root_post rp;
	
	-- Case statement to determine how many replies to load, depth of each tree branch, how many recursions per depth
	CASE
		WHEN parent_num_of_replies <= 25 THEN
     		direct_reply_limit := 25;
			depth_limit := 6;
			recursive_limit := 4;
		WHEN parent_num_of_replies BETWEEN 26 AND 80 THEN
			direct_reply_limit := 40;
			depth_limit := 4;
			recursive_limit := 2;
		WHEN parent_num_of_replies > 80 THEN
			direct_reply_limit := 60;
			depth_limit := 4;
			recursive_limit := 1;
		-- Case statement breaks if you send a bad ID
	END CASE;
	
	--combine and return results
	RETURN QUERY
 	SELECT * FROM root_post
	UNION ALL
	SELECT * FROM get_replies_by_parent_id(input_id, direct_reply_limit, depth_limit, recursive_limit);
	
	--RAISE NOTICE '% % % %', parent_num_of_replies, direct_reply_limit, depth_limit, recursive_limit;
	-- 	FOR items IN SELECT * FROM joined_results LOOP
	--         RAISE NOTICE '%', items;
	--  END LOOP;
	
	DROP TABLE root_post;
	RETURN;
			
END; 
$$
LANGUAGE 'plpgsql';
-- ## Lower Bound of Wilson Score Confidence Interval
CREATE FUNCTION ci_lower_bound(upvotes INT, downvotes INT)
RETURNS NUMERIC(5)
AS 
$$
BEGIN
	RETURN ((upvotes + 1.9208) / (upvotes + downvotes) - 
             1.96 * SQRT((upvotes * downvotes) / (upvotes + downvotes) + 0.9604) / 
             (upvotes + downvotes)) / (1 + 3.8416 / (upvotes + downvotes));
END; 
$$
LANGUAGE 'plpgsql';
-- ## Get Root Replies with CI Lower Bound
CREATE FUNCTION get_root_post_replies_w_algorithm(input_parent_id INT)
RETURNS TABLE(id INT, parent_id INT, upvotes INT, downvotes INT, author VARCHAR, create_date TIMESTAMP, body TEXT, depth INT, num_of_replies BIGINT)
AS
$$
BEGIN
	RETURN QUERY
	
	WITH RECURSIVE posts_tree AS (
	  SELECT
		 p.*,
		 0 depth,
		 ci_lower_bound(upvotes, downvotes) AS ci_lower_bound
	  FROM posts p
		WHERE p.parent_id = input_parent_id
	  UNION ALL
	  SELECT
		 p.*,
		 pt.depth + 1,
		 ci_lower_bound(p.upvotes, p.downvotes)
	  FROM posts p
		JOIN posts_tree pt ON p.parent_id = pt.id
	)

	SELECT
		ptree.id,
		ptree.parent_id,
		ptree.upvotes,
		ptree.downvotes,
		ptree.author,
		ptree.create_date,
		ptree.body,
		ptree.depth,
		(SELECT COUNT(p.id) FROM posts p WHERE p.parent_id = ptree.id) AS num_of_replies
	FROM posts_tree ptree
	ORDER BY ptree.ci_lower_bound;
END;
$$
LANGUAGE 'plpgsql';

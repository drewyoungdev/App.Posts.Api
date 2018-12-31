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
('6', '4', '100', '1', 'user1', '2010-08-20T15:00:00', 'Ranked higher with lower average rating (99% positive)'),
('7', NULL, '10', '5', 'user1', '2010-08-20T15:00:00', 'This is a root-post #3'),
('8', '7', '10000', '1', 'user1', '2010-08-20T15:00:00', 'Very HIGH confidence bound'),
('9', '7', '1', '10000', 'user1', '2010-08-20T15:00:00', 'Very LOW confidence bound'),
('10', NULL, '10', '5', 'user1', '2010-08-20T15:00:00', 'This is a root-post #4'),
('11', '10', '10', '25', 'user1', '2010-08-20T15:00:00', 'Low Reply to Post 10'),
('12', '10', '100', '5', 'user1', '2010-08-20T15:00:00', 'Medium Reply to Post 10'),
('13', '10', '101', '4', 'user1', '2010-08-20T15:00:00', 'High Reply to Post 10'),
('14', '11', '5500', '4500', 'user1', '2010-08-20T15:00:00', 'Low Reply to Post 11'),
('15', '11', '0', '0', 'user1', '2010-08-20T15:00:00', 'Medium Reply to Post 11'),
('16', '11', '600', '400', 'user1', '2010-08-20T15:00:00', 'High Reply to Post 11'),
('17', '10', '102', '3', 'user1', '2010-08-20T15:00:00', 'Highest Reply to Post 10'),
('18', '10', '0', '3000', 'user1', '2010-08-20T15:00:00', 'Lowest Reply to Post 10');
-- ## Lower Bound of Wilson Score Confidence Interval
--The determines the likelihood of another "upvote" based on the binomial distribution of upvotes to downvotes
--Currently this is hardcoded for a 95% confidence level http://www.evanmiller.org/how-not-to-sort-by-average-rating.html
--Node.js implementation: https://gist.github.com/timelf123/dadca20e7faa17969d3eb6ee375e2c98
CREATE FUNCTION ci_lower_bound(upvotes INT, downvotes INT)
RETURNS NUMERIC(5)
AS 
$$
BEGIN
	IF (upvotes + downvotes = 0)
	THEN
		RETURN 0;
	ELSE
		RETURN ((upvotes + 1.9208) / (upvotes + downvotes) - 
				1.96 * SQRT((upvotes * downvotes) / (upvotes + downvotes) + 0.9604) / 
				(upvotes + downvotes)) / (1 + 3.8416 / (upvotes + downvotes));
	END IF;
END; 
$$
LANGUAGE 'plpgsql';
-- ## Get Root Replies with CI Lower Bound
-- Always filter down negatively rated comments
-- Begin limiting direct comments or only ever showing ones with a certain ci_lower_bound value?
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
		 ARRAY[-ci_lower_bound(p.upvotes, p.downvotes)] path
	  FROM posts p
		WHERE p.parent_id = input_parent_id
        -- AND p.upvotes > p.downvotes -- only postively rated comments
		-- LIMIT limit total number of direct replies
        -- Or accept an OFFSET to continue through list
	  UNION ALL
	  SELECT
		 p.*,
		 pt.depth + 1,
		 pt.path || ARRAY[-ci_lower_bound(p.upvotes, p.downvotes)]
	  FROM posts p
		JOIN posts_tree pt ON p.parent_id = pt.id
		-- WHERE p.upvotes > p.downvotes -- only postively rated comments
		-- WHERE pt.depth + 1 <= 6 -- limit depth
		-- LIMIT 5 -- limit to only pulling top 5 rated comments
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
	ORDER BY ptree.path;
END;
$$
LANGUAGE 'plpgsql';
-- NEW: fn to just get root-post replies without algorithm (get_root_post_replies) (old way by just overall score (upvotes - downvotes))
-- NEW: fn to get all replies to a parent post (get_all_replies) (used to load replies after algorithm) + one depth and one reply
-- NEW: fn to just get root-post (get_root_post) (FE can load root post and it's meta data asynchronously while loading replies)

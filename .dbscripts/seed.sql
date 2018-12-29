\c posts_db
--### Create posts table
CREATE TABLE IF NOT EXISTS posts (
  id          serial primary key,
  parent_id   integer,
  score     integer,
  author      varchar(20),
  create_date timestamp,
  comment     text
);
--### Insert dev records
INSERT INTO posts (parent_id, score, author, create_date, comment) VALUES 
(NULL, '10', 'user1', '2010-08-20T15:00:00', 'This is a mid-tier comment'),
(NULL, '0', 'user1', '2010-08-20T15:00:00', 'This is a low-tier comment'),
(NULL, '30', 'user1', '2010-08-20T15:00:00', 'This is a high-tier comment'),
('1', '5', 'user1', '2010-08-20T15:00:00', 'Reply to "1" mid-tier comment'),
('1', '12', 'user1', '2010-08-20T15:00:00', 'Reply to "1" high-tier comment'),
('1', '0', 'user1', '2010-08-20T15:00:00', 'Reply to "1" low-tier comment'),
('5', '12', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
('5', '12', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
('5', '0', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
('5', '5', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
('5', '5', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
('8', '0', 'user1', '2010-08-20T15:00:00', 'Reply to "8" comment'),
('8', '2', 'user1', '2010-08-20T15:00:00', 'Reply to "8" comment'),
('8', '3', 'user1', '2010-08-20T15:00:00', 'Reply to "8" comment'),
('5', '2', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment'),
(NULL, '3', 'user1', '2010-08-20T15:00:00', 'Reply to "8" comment'),
('16', '0', 'user1', '2010-08-20T15:00:00', 'Level 1'),
('17', '0', 'user1', '2010-08-20T15:00:00', 'Level 2'),
('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 3'),
('19', '0', 'user1', '2010-08-20T15:00:00', 'Level 4'),
('20', '0', 'user1', '2010-08-20T15:00:00', 'Level 5'),
('21', '0', 'user1', '2010-08-20T15:00:00', 'Level 6'),
('22', '0', 'user1', '2010-08-20T15:00:00', 'Level 7'),
('17', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.1'),
('21', '0', 'user1', '2010-08-20T15:00:00', 'Level 6.1'),
('21', '0', 'user1', '2010-08-20T15:00:00', 'Level 6.2'),
('19', '0', 'user1', '2010-08-20T15:00:00', 'Level 3.1'),
('19', '0', 'user1', '2010-08-20T15:00:00', 'Level 3.2'),
('19', '0', 'user1', '2010-08-20T15:00:00', 'Level 3.3'),
('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.1'),
('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.2'),
('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.3'),
('18', '0', 'user1', '2010-08-20T15:00:00', 'Level 2.4');
--### Create function to get post tree
CREATE FUNCTION get_posts_tree_by_id(input_id INT, sub_level_limit INT)
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
		 ARRAY[-p.score, p.id] path,   -- used to sort by vote then ID
	     ROW_NUMBER() OVER (ORDER BY p.score DESC, p.id) row_num
	  FROM posts p
		-- Filter for all generations of children of parent (e.g. when you enter a main thread)
		WHERE p.id = input_id
		ORDER BY p.score DESC, p.id ASC
	  )
	  UNION
	  -- Self referential select performed repeatedly until no more rows are found
	  (SELECT
		 p.id,
		 p.parent_id,
		 p.author,
		 p.create_date,
		 p.score,
		 p.comment,
		 pt.depth + 1,
		 pt.path || ARRAY[-p.score, p.id],
	     ROW_NUMBER() OVER (ORDER BY p.score DESC, p.id)
	  FROM posts p
		JOIN posts_tree pt ON p.parent_id = pt.id
	    --This limits the number of recursive joins made at each depth and limits the depth.
		--Retrieve top 20 posts per depths under 7.
		WHERE pt.depth <= 6
	   	LIMIT sub_level_limit
		--TODO: Figure out how to do conditional limits based on depth (e.g. at depth 6 we only want LIMIT 1)
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
	--Gather count of replies (Query to find out how many other posts reference current one as parent_id)
	(SELECT COUNT(p.parent_id) FROM posts p WHERE p.parent_id = ptree.id) AS num_of_replies
	FROM posts_tree ptree
	--Filter results to limit the amounts per depth (and reduce unnecessary counts)
	--This is performance on midware and reduces response payloads to FE clients
	WHERE (ptree.depth = 0 AND ptree.row_num <= 20) 
			OR (ptree.depth = 1 AND ptree.row_num <= 10) 
			OR (ptree.depth = 2 AND ptree.row_num <= 5)
			OR (ptree.depth = 3 AND ptree.row_num <= 3)
			OR (ptree.depth = 4 AND ptree.row_num <= 1)
			OR (ptree.depth = 5 AND ptree.row_num <= 1)
			OR (ptree.depth = 6 AND ptree.row_num <= 1)
	ORDER BY ptree.path;

END;
$$ 
LANGUAGE 'plpgsql';
--### Create function to get reply counts per post
-- CREATE FUNCTION get_num_of_replies_by_ids(input_ids INT[])
-- RETURNS TABLE(id INT, num_of_replies BIGINT)
-- AS 
-- $$
-- BEGIN
-- 	RETURN QUERY
	
-- 	SELECT 
--   p.parent_id, 
-- 	COUNT(p.parent_id) AS num_of_replies 
-- 	FROM posts p 
--   WHERE p.parent_id = ANY(input_ids)
--   GROUP BY p.parent_id;
	
-- END;
-- $$ 
-- LANGUAGE 'plpgsql';

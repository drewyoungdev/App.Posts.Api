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
('5', '2', 'user1', '2010-08-20T15:00:00', 'Reply to "5" comment');
--### Create function to get post tree
CREATE FUNCTION get_posts_tree_by_parent_id(input_id INT, sub_level_limit INT)
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
		 ARRAY[-p.score, p.id] path   -- used to sort by vote then ID
	  FROM posts p
		-- Filter for all generations of children of parent (e.g. when you enter a main thread)
		WHERE p.id = input_id
		-- Limit the initial list of posts loaded per main thread
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
		 pt.path || ARRAY[-p.score, p.id]
	  FROM posts p
		JOIN posts_tree pt ON p.parent_id = pt.id
		-- Every depth we want to sort by score then id then only apply recursion to top x posts (3 to test) 
		-- Note: this cannot be used if base criteria filters on parent_id IS NULL 
		-- because it will prevent any other 'parent' nodes from finding their children after the LIMIT is consumed
		ORDER BY p.score DESC, p.id ASC 
		LIMIT sub_level_limit
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
	(SELECT COUNT(p.parent_id) FROM posts p WHERE p.parent_id = ptree.id) AS num_of_replies
	FROM posts_tree ptree 
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

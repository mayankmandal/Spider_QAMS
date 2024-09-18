ALTER VIEW vwUserPagesData AS
WITH CombinedData AS (
  -- For Pages Data
  SELECT 
    DISTINCT 
    up.ProfileId,
    p.PageId,
    p.PageUrl,
    p.PageDesc,
    p.PageCatId,
    CategoryName = NULL,
	upro.UserID
  FROM UserPermission up WITH (NOLOCK) 
  INNER JOIN tblPage p WITH (NOLOCK) ON p.PageId = up.PageId
  INNER JOIN UserProfile upro WITH (NOLOCK) ON up.ProfileId = upro.ProfileID

  UNION  

  -- For Category's Pages Data
  SELECT 
    DISTINCT 
    u.ProfileId,
    p.PageId,
    p.PageUrl,
    p.PageDesc,
    p.PageCatId,
    tpc.CategoryName,
	tbup.UserId
  FROM UserPermission u WITH (NOLOCK) 
  INNER JOIN tblPage p WITH (NOLOCK) ON p.PageId = u.PageId
  INNER JOIN tblPageCategory tpc WITH (NOLOCK) ON p.PageCatId = tpc.PageCatId
  INNER JOIN UserProfile tbup WITH (NOLOCK) ON tbup.ProfileID = u.ProfileId 
)

SELECT DISTINCT
  cd.ProfileId,
  cd.PageId,
  cd.PageUrl,
  cd.PageDesc,
  cd.PageCatId,
  cd.CategoryName,
  cd.UserId
FROM CombinedData cd
WHERE NOT EXISTS (
  SELECT 1
  FROM CombinedData sub
  WHERE cd.PageId = sub.PageId
    AND sub.PageCatId IS NOT NULL
    AND sub.CategoryName IS NOT NULL
)
OR
(
  cd.PageCatId IS NOT NULL
  AND cd.CategoryName IS NOT NULL
);

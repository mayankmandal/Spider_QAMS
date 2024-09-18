ALTER VIEW vwUserPageAccess AS
-- For Direct Pages Data
SELECT 
  DISTINCT 
  u.ProfileId,
  p.PageId,
  p.PageUrl,
  p.PageDesc,
  tbup.UserId
FROM UserPermission u WITH (NOLOCK) 
INNER JOIN tblPage p WITH (NOLOCK) ON p.PageId = u.PageId
INNER JOIN UserProfile tbup WITH (NOLOCK) on tbup.ProfileId = u.ProfileId 

UNION

-- For Category's Pages Data
SELECT 
  DISTINCT 
  u.ProfileId,
  p.PageId,
  p.PageUrl,
  p.PageDesc,
  tbup.UserId
FROM UserPermission u WITH (NOLOCK) 
INNER JOIN tblPage p WITH (NOLOCK) ON p.PageId = u.PageId
INNER JOIN tblPageCategory pc WITH (NOLOCK) ON pc.PageCatId = p.PageCatId
INNER JOIN UserProfile tbup WITH (NOLOCK) on tbup.ProfileID = u.ProfileId 

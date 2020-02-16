# SqlLockFinder
Is a visual aid to find locks in your database.
Each connected session will be represented by a circle.  When sessions are locking each other then these sessions will connected with a line.

## Features
- Visual representation of locking sessions (with pretty moving circles)
- Find sessions by SPID
- Kill locking sessions
- View exact locked rows with row content
- View what sessions are exactly waiting for when suspended by another sessions lock

## Supported locks
- Table
- Page
- Key
- RID



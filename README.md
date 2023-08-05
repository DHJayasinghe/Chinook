# Chinook

This application is unfinished. Please complete below tasks. Spend max 2 hours.
We would like to have a short written explanation of the changes you made.

1. Move data retrieval methods to separate class / classes (use dependency injection)
2. Favorite / unfavorite tracks. An automatic playlist should be created named "My favorite tracks"
3. The user's playlists should be listed in the left navbar. If a playlist is added (or modified), this should reflect in the left navbar (NavMenu.razor). Preferrably, the left menu should be refreshed without a full page reload.
4. Add tracks to a playlist (existing or new one). The dialog is already created but not yet finished.
5. Search for artist name

When creating a user account, you will see this:
"This app does not currently have a real email sender registered, see these docs for how to configure a real email sender. Normally this would be emailed: Click here to confirm your account."
After you click 'Click here to confirm your account' you should be able to login.

Please put the code in Github. Please put the original code (our code) in the master branch, put your code in a separate branch, and make a pull request to the master branch.



## What has Changed
1. Moved c# code logics in UI to seperate _.Razor.cs classes to keep the UI & backend logics clean & seperate.
2. Moved Data read & write logics to 3 different services. DBContext is accessible only from their
3. Used C# Events to notify Nav bar when there is a new Playlist added
4. Used OnParametersSet override on PlaylistPage to detect Parameter changed and update the UI based on the PaylistId
5. Favorite Playlist adding logic (if not exist) added to NavPage load. If there is no Favorite playlist, one will be created for the user.
6. Navigation playlists ordered to show the Favorite playlist first
7. Artist Searchbar works on when pressing Enter key

## IMPORTANT Performance Improvement Suggestions 
1. Implement Indexing for Artist Search - The below EFCore query we wrote for Search artist by Name is doing a Clustered Index Scan. Which is slow performance indicator for Databases. 
`_dbContext.Artists.Where(artist => artist.Name.Contains(artistName))`
Also the SQL query it generates is as below 
`SELECT "a"."ArtistId", "a"."Name"
      FROM "Artist" AS "a"
      WHERE @__artistName_0 = '' OR instr("a"."Name", @__artistName_0) > 0`
Which is **NON-SARGable Predicate** which gonna skip indexes if there any. Right now the Artist table is small & just contain one field. So it's fine. But in real world, this might have more columns and millions of rows, when that comes it's require to have an index. And the query we wrote will have a bad performance impact. So should consider moving to procedure and write SARGable predicate search artist. Ex: Name LIKE 'John%' OR use things like Filtered Indexes.
2. Pagination for Artists & Playlist - Currently we are loading all the Artists into the UI. Which is unncessary. User is not gonna view all these from one shot. Pagination can reduce server resouce consumptions and improve responsiveness.
3. Implement Explicit Loading - Below code line has to load all the Track entities of Favorite playlist into memory to tell the EFCore to say that just one entity has removed. If there are 100 tracks in my Favorite Playlist and If i want to remove one, that mean I have load all the 100 entities. Using explicit loading can prevent this and just load the item we want to remove.
![image](https://github.com/DHJayasinghe/Chinook/assets/26274468/be8156dd-6269-48d5-86fe-51c440da101a)

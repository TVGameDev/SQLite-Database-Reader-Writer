# SQLite-Database-Reader-Writer
An ongoing effort to create an easy-to-use database system for saving and loading values to Unity games.

#How to Edit DB files?
I highly recommend DBBrowser for SQLite found here : http://sqlitebrowser.org/

#Why aren't my changes to the database showing up in-game?
When the game runs for the first time, it creates a file in your persistent data path that is a renamed COPY of the one found in your StreamingAssets. If you want to see live changes to the file make modifications at the db file found at C:\Users\[USER]\AppData\LocalLow\DefaultCompany\DBManager (remember to change the [USER] to your user folder) OR you can delete that file and allow it to re-initialize from the modified original in your streamingAssets.

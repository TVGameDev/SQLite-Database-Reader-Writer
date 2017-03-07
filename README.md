# SQLite-Database-Reader-Writer
An ongoing effort to create an easy-to-use database system for saving and loading values to Unity games.

#How to Edit DB files?
I highly recommend DBBrowser for SQLite found here : http://sqlitebrowser.org/

#Why aren't my changes to the database showing up in-game?
When the game runs for the first time, it creates a file in your persistent data path that is a renamed COPY of the one found in your StreamingAssets. If you want to see live changes to the file make modifications at the db file found at C:\Users\[USER]\AppData\LocalLow\DefaultCompany\DBManager (remember to change the [USER] to your user folder) OR you can delete that file and allow it to re-initialize from the modified original in your streamingAssets.

#What modifications can I make to the Database?
Technically you can do whatever you want to it, BUT only certain fields will show up in the game's console. While the scripts for reading and writing to the database are generic, the script that's loading data to the console is not. Instead, it's looking up specifically named fields in each table to get their data. If you want to change what shows up on-screen, make changes inside the 'GameBehavior' script or in another that uses my DB classes.

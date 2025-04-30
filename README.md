# Minesweeper
A MAUI minesweeper application

this code uses .NET MAUI with .net 8

to run/use the code in microsoft visual studio code

 open the project, wait for the project to finish opening (sometimes gets weird if you try doing stuff during visual studio code's opening processes)
 run the project's MainPage.xaml.cs 

 then select a difficulty to play mine sweeper on
  difficulty ONLY changes the board size and mine count

Left click a tile to make a guess

the first click will always be safe and the surrounding tile will never have a mine

right click a tile to flag it
  right click again to mark as amigous
    one more right click will set tile back to normal

you can right click a revealed number tile
  IF the number tile has an equal number of flags to its value then it will reveal the surrounding tiles that are not flaged
    this can reveal mines if a flag is wrong

if at any point a mine is revealed you lose

if you reveal all tiles that are not mines you win

This is an extended version of the Tic Tac Toe game found in this awesome tutorial: 
https://www.youtube.com/watch?v=OHRWRpT9WcE

(For those of you who are unaware, OttoBotCode is a youtuber/Patreon member from Denmark, and he makes EXCELLENT videos and tutorials. His code is clean and simple. I can't recommend enough.)

The main additions to this game are: 
- Options menu
- play against CPU with various levels

This *DOES NOT* leverage MVVM in any meaningful way; things are tightly coupled as this was a simple for-fun project I wanted to toy with. There are still optimizations and code cleanup I would like to do, but it is currently functional. 
It will be a little bit before I revisit and better segregate for SOLID, though admittedly where this isn't much of a complicated project, I don't know that I'll change much other than the final bug fixes. 

Feel free to take this project and extend it further if you'd like, or if you have bug fixes/suggestions, let me know. 

The AI leverages the minimax alg (easy is RNG, medium is 1-move lookahead, while hard is a perfect game.)

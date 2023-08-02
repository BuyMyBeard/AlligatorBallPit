# Alligator Ball Pit
 GMTK 2023 game jam submission.
 
 Out of 6821 entries, we placed #301 overall! Woohoo!
 
[Playable here!](https://buymybeard.itch.io/alligator-ball-pit)
 
![Game Thumbnail](https://img.itch.zone/aW1nLzEyNzQ4MjIxLmdpZg==/original/ZUguUT.gif)

This game was made in 48 hours with the help of this wonderful team:
- [**Rebecca Harrie**](https://www.instagram.com/rebecca_harrie/), Artist
- [**SmallOcelot**](https://smallocelot.itch.io/), Artist, Programmer
- [**Andre Roz**](https://andreroz.com/), Audio Engineer

Rebecca and Andre were part of the making of Ratatime, so we decided to go with the same approach of using a git submodule for keeping all intellectual property in a private repository.

## Implementation Challenge

The biggest challenge this jam was definitely the time constraint. 48 hours is not a lot, especially with multiple people all on different timezones (Mexico, Canada, Australia, United Kingdom). 

What really helped for me is having scripts and UI prefabs pretty much ready from other projects. The player physics was originally made for Ratatime, but adapted over 2 other projects. It has less features than in Ratatime (no slope compensation and one-way platforms), but the ground check is now a small box instead of a raycast, preventing slipping on edges of tiles while still providing edge rounding.

We also didn't have a level designer, so SmallOcelot took the mantle of bringing our ideas into well realized levels. There is more polish that could have been added and some level design issues that are apparent to me after watching others play, but I think it holds well for a game made in 48 hours.

## Game Design Challenge

The other big challenge we had was definitely to come up with an idea. The theme of the jam was **Roles Reversed**, and I had made a game with a similar theme 2 weeks prior (The theme was **You Are The Monster** and I made [Scare Rosalyn](https://github.com/BuyMyBeard/Scare-Rosalyn), a reverse horror game inspired by the Hitman series).

The brainstorming session lasted a good 3 hours, where me and Rebecca had a back and forth of ideas and we ended up scrapping a bunch of them. There was a bunch of limitations, like obviously the time constraint, but also the complexity of the programming. It was easy to come up with ideas for this theme, but most ideas were either unfeasible, unfun, or both. Two main ideas we had were a reverse dating visual novel game and a tile-based puzzle game where materials have reverse properties. We ended up making a gag 2d platformer game, where we implemented ideas from the 2 other games, like a dialogue system, and the materials with reverse properties.

I think the pitfall a lot of people fell into for the theme is that a lot of ideas devolve into making an AI for the usual role of the player, which can be a big challenge in 48 hours. It's basically what I did for [Scare Rosalyn](https://github.com/BuyMyBeard/Scare-Rosalyn), where I made the player AI controlled in a horror game, but I had a lot more time for that jam.

The other pitfall I saw and didn't want to fall into was that it was easy to take a game, reverse something about it, but in the process make the game unplayable or unfun. The design of most games sees ideas meshed together to solve design problems. A system will bring a problem, but another system will come and help solve that problem while maybe bringing it's own. Playing around with that can be tricky, and I saw some other submissions that fell for it. I played a peggle game where instead of toggling off the tiles your ball touches, you toggle them on. Long story short it was a painful to play.

Anyway, have fun browsing the code!

# NEOPixelBattle
The first one of this repository is the NEOPixelBattle
I wanted to bring the pixel battle game like into the NEO Blockchain, wich is very interesting because ou can follow all the changes in the table since the deploy.

## Rules:
- Is a shared white table with a million pixel to everyone on the network play.
- Any address can send an color twice in a roll, must some other player put a color inside the board before you go!

- SetColorRGB() paint a pixel with the choosen color
    - The i and j refers to the matrix position of the pixel
    - The color is a hex format like **#ffffff**
    - The address is the address of the caller
	
- GetColorRGB() returns color of a pixel     
    - The i and j refers to the matrix position of the pixel
	
- Colored is an event that is emited every time that one address change the color of a pixel


## Thanks

Thanks to the [neocompiler.io](https://neocompiler.io/) team that make the shared private net usefull for tests. I was able to create just a front end to comunicate with the SmartContract i made, since when i created the game i had no intention to create an interface, not even deploy it since i made it just for Fun.

Thanks to the [Liqing Pan](https://github.com/dprat0821) for discussing many ideas that could help our both projects, one specifically is the multi-random number algorithm in the block and, of course, encourage me to submit the game i had done on my github page.

Also great thanks to the [norchain.io](https://github.com/norchain) team that create [Neunity](https://github.com/norchain/Neunity) tool that really helped me.

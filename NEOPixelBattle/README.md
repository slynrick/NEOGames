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
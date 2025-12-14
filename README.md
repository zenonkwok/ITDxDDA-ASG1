# ITDxDDA_ASSG1
Repository for ITD x DDA Assignment 1

# Controls:
* WASD to move
* Left-Click on mouse to look around
* Right Click to interact with UI buttons & grab the review notepads 

# Goal of the app:
To assist students in making the right decision on the place to eat as well as what food item to eat/order!!!

# Platform: 
Android Smartphones

# Known bugs:
NIL aka we havent encountered any so far... 

# Content Displayed
## Login Scene
1. Logo of app
2. Name of app
3. Slogan
4. Email input field
5. Password input field 
6. Sign up button
7. Login button

## Sign up page 
1. Logo of app
2. Name of app
3. Slogan
4. Email input field
5. Password input field 
6. Login button
7. Sign up button 

## Main Menu Scene
1. Logo of app
2. Name of app
3. Review/Foodie mode button
4. About us button

## About Us Page
1. Developers Header 
2. One picture of each developer
3. Description on why we made Forked
## Review Scene 
### Reviewing the food stall
1. Rate This Place button
2. 5 emojis 
3. Remarks input field
4. Submit button

### Seeing the reviews from other users about the food stall
1. Read Reviews button
2. 4 notepads - 2 good reviews and 2 bad reviews - containing different info - username, customer rating and remarks. 

### Seeing food stats of the food items
1. 3D model of the food item
2. Notepad containing info - allergy content, taste, description of food and overall customer rating - about the food item that is pulled from the firebase realtime database.

# Step by Step Instructions for each scene:
## Login Scene
1. Login using a real email and password you have set when signing up.
2. Click on the Login Button. 
3. If you are not sent to the main menu, you have entered the wrong password and/or email.

**If you have not made a account please press the Sign Up button at the bottom of the page and it will send you to the sign up page**

## Main Menu Scene
1. To access the main aspect of the app, click on Review/Foodie Mode and it will send you to the ReviewScene
2. Clicking on the About Us button will lead you to a page that shows you the developers and a short description on why we made the app. To return to the Main Menu, click on the Back to Main Menu button. 

## Review Scene
### Making a review of the food stall
1. Hover over the logo of the stall and the notepad should appear and select the Rate This Place button.
2. Select which emoji suits your current feelings towards the food stall.
3. Click on the remarks input field to type in any remarks you have. 
4. Click on the Submit button.
5. You will be redirected and to return to the frst page of the notepad, press the Return to main menu button.

### Seeing other user's reviews of the food stall
1. Hover over the logo of the stall and the notepad should appear and select the Read Reviews button.
2. 3 other notepads with user's reviews will spawn around the original notepad. 
4. To go back to the main page of the notepad, click on the back button on the original notepad.

### Seeing the Food Stats of certain food items 
1. Hover over the picture of the food item you want to know more about.
2. After hovering, a 3d model of the respective food item will appear with a notepad detailing the allergy content, taste, description and overall customer rating of the food.
3. To despawn the 3d model, simply look away from the picture of the food item.

# Limitations
The overall customer rating seen on the food stats notepad is not calculated based on the reviews made by users and is just hard coded in the firebase realtime database

# Credits:

ChatGPT in debugging and creation of images

Models used:
* Chicken Rice: https://sketchfab.com/3d-models/singapore-chicken-rice-17178469a639453491ad907195cd443d
* Chicken Kebab: https://sketchfab.com/3d-models/kebab-0dad8550cccb4149a12f1f393e64b4b3
* Baked Rice: https://sketchfab.com/3d-models/scan-2023-04-12t231302275z-19d17a098f224d41a677a9e25998e2b5
Sfx used:
* BGM Used: https://youtu.be/gcVzc-yJ1KY?si=oSxRPj6l66gYjok8
* Clicking of button SFX: https://pixabay.com/sound-effects/button-click-289742/

# Original 3D Models Created
* All emojis modeled by Ras
* Notepad modeled by Zenon 

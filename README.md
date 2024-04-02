# SuXo - Hyper-casual Unity-Sui Blockchain Game

SuXo is a game based on old PC game called Xonix. SuXo leverages the power of the Sui blockchain through the Unity-Sui wallet integration developed by ALL.ART. This document provides an overview of how to install SuXo and how it integrates with the Sui blockchain.

## Overview

Drawing inspiration from the classic Xonix game, SuXo offers a unique blend of strategy and skill-based gameplay, enhanced by its integration with the Sui blockchain. Players navigate a character across the playing field, carving out sections to capture territory while avoiding enemies. The integration with the Sui blockchain enriches the gaming experience by enabling players to use Sui blockchain assets for in-game transactions and interactions, all facilitated through the Unity-Sui wallet.

As players progress through SuXo, successfully completing levels rewards them with coins. These coins are not just a measure of success but also a valuable in-game currency that can be used to unlock access to new levels. This mechanic adds an extra layer of strategy, as players must decide whether to save their coins for future levels or spend them on immediate advantages.

In addition to unlocking levels, coins can also be used to purchase extra lives. In the challenging world of SuXo, losing all lives means game over, but with extra lives, players have the opportunity to continue from where they left off. This feature not only enhances the gameplay experience by allowing players to progress further but also adds a tactical element, as players must manage their resources wisely to advance through the game.

## Installation

To get started with SuXo and integrate it with the Sui blockchain using the Unity-Sui wallet, follow these steps:

1. **Clone the SuXo Repository**: First, clone the SuXo repository to your local machine using Git. You can do this by running the following command in your terminal or command prompt:
   ```
   git clone https://github.com/allartprotocol/suxo-unity
   ```

2. **Open the Project in Unity**: Open Unity Hub, click on 'Add' and navigate to the cloned SuXo project directory. Select the project to add it to your Unity Hub and then open it.

3. **Add Unity-Sui Wallet Package**: The Unity-Sui wallet package needs to be added to your project manually from disk. Follow these steps to add it:
   - Clone the Unity-Sui wallet repository from ALL.ART. You can find the repository [here](https://github.com/allartprotocol/allart-unity-sui-wallet).

To import the Unity-Sui wallet package into your project, you will need to select the `package.json` file within the cloned Unity-Sui wallet repository. This file contains all the necessary information for Unity to recognize and import the package into your project. Follow the detailed steps below:

- Navigate to the cloned Unity-Sui wallet repository on your local machine.
- Locate the `package.json` file within the repository. This file is essential for Unity to understand how to integrate the package into your project.
- In Unity, with your project open, go to the Package Manager window.
- In the Package Manager, select the '+' icon, then choose 'Add package from disk...'.
- In the file dialog that opens, navigate to the location of the `package.json` file within the Unity-Sui wallet repository, select it, and click 'Open'.
- Unity will now import the Unity-Sui wallet package into your project, making it available for use.

By selecting the `package.json` file for import, you ensure that Unity correctly integrates the Unity-Sui wallet package into your project, allowing you to proceed with configuring and using the wallet for blockchain interactions in your game.

1. **Configure the Project**: After importing the Unity-Sui wallet package, you may need to configure your project settings. This typically involves setting up the wallet connection and specifying any required identifiers or keys.

To fully integrate SuXo with the Sui blockchain, it's essential to deploy the game's contract logic to the Sui network (either devnet or mainnet). This process involves using the SuXo-Sui Git repository, which contains all necessary Sui contract logic. Follow the steps below to deploy your contracts and configure your project:

**Deploy Contracts to Sui**: Navigate to the [SuXo-Sui Git repository](https://github.com/allartprotocol/suxo-sui), which contains the Sui contract logic. Use the provided scripts or instructions to deploy these contracts to your desired network (devnet or mainnet). Note the transaction IDs and object IDs generated during this process.

**Configure `SUINetworking.cs`**: After deploying the contracts, you will receive object IDs for each deployed contract. These object IDs are crucial for the game's interaction with the Sui blockchain. Open the `Assets/App/Scripts/SUINetworking.cs` file in your project and replace the placeholder values for `GAME_PACKAGE`, `GAME_ID`, `ADMIN_CAP`, `LEVEL_CAP`, `SCORE_CAP`, `LIFE_CAP`, and any other relevant constants with the actual object IDs obtained from the deployment process.

By following these steps, you ensure that SuXo is correctly integrated with the Sui blockchain, allowing for seamless blockchain interactions within the game. Remember to test the integration thoroughly to ensure everything is working as expected.


1. **Build and Run**: Once everything is set up, you can build and run the SuXo game. Test the integration by performing transactions or interacting with the Sui blockchain through the game.


### Key Components

- **SUINetworking.cs**: This script is crucial for the game's interaction with the Sui blockchain. It contains constants representing object identifiers on the Sui blockchain, such as `GAME_PACKAGE`, `GAME_ID`, and various capabilities like `SCORE_CAP` and `LIFE_CAP`. These identifiers are used in blockchain transactions to create players, increase scores, and manage game assets.

- **EnemyController.cs**: This script is responsible for managing the behavior of enemies within the game. It controls their movement, handles collisions, and manages interactions with the game environment using a grid system to navigate and detect collisions with the player's path.

- **SpyController.cs**: Similar to the EnemyController, this script manages the spies in the game. Spies are a special type of enemy with unique behaviors and interactions within the game environment.

- **PlayerController.cs**: This script manages the player's behavior, including movement and interactions with the game environment. It processes input for directing the player's movement and handles collisions with enemies and other game elements.

- **GameView.cs**: This script is responsible for managing the overall game view, including transitioning between different screens such as the main menu, game over screen, and the gameplay view itself. It handles the visibility and interactions of UI elements across the game.

- **CongratsScreen.cs**: This script manages the congratulations screen that appears when a player successfully completes a level or achieves a significant milestone in the game. It displays messages of encouragement, level completion stats, and options to proceed to the next level or revisit the main menu.

- **LandingScreen.cs**: This script controls the landing or main menu screen of the game. It's the first interface a player interacts with upon launching the game.

- **GameOverScreen.cs**: This script manages the game over screen, allowing players to buy lives with Sui coins, use existing lives to continue playing, or restart the game. It interacts with the `GameManager` to handle these actions.

- **GameManager.cs**: The central script that manages the game's state, including starting the game, playing levels, spawning enemies, and handling player deaths. It uses `SUINetworking` to interact with the Sui blockchain for tasks such as playing levels and managing player assets.


### Grid Management

The `GridManager.cs` script is a crucial component of the game's architecture, responsible for managing the grid system that forms the basis of the game's play area. This script handles the creation, management, and interaction of cells within the grid, which are used to navigate and control the movement of both the player and enemies within the game. Key functionalities include:

- **Grid Initialization**: At the start of the game or a level, `GridManager` initializes the grid based on predefined dimensions, creating a matrix of cells that represent the playable area.

- **Cell Management**: Each cell within the grid has a state (e.g., empty, fillable, path, claimed) that determines how it interacts with the player and enemies. `GridManager` updates these states as the game progresses, based on player movement and game events.

- **Player and Enemy Navigation**: The script provides methods to determine valid movement directions for the player and enemies based on the current grid state. It ensures that movements are constrained within the grid boundaries and handles collisions and pathfinding.

- **Camera Adjustment**: To ensure optimal visibility, `GridManager` adjusts the camera's position and zoom based on the grid's size and the player's position within the grid.

- **Utility Methods**: The script offers various utility methods, such as converting grid coordinates to world space positions, checking cell states, and finding paths within the grid.


### Blockchain Integration

The game's integration with the Sui blockchain is primarily handled through the `SUINetworking.cs` script. It facilitates creating players, managing scores and lives, and interacting with game assets on the blockchain. The script uses the Sui RPC client to make calls to the blockchain, handling transactions and responses.

### Unity-Sui Wallet

The Unity-Sui wallet, developed by [ALL.ART](https://all.art), plays a pivotal role in the blockchain integration of SuXo. It enables players to securely manage their assets and execute transactions on the Sui blockchain without leaving the game. This integration is designed to be seamless, offering a straightforward and intuitive interface for all Unity-Sui blockchain-related activities.



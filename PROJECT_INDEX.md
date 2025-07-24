# FreeRun Project Index

## Project Overview
**FreeRun** is a Unity-based multiplayer game experiment focused on dedicated server architecture. It's a running/parkour game with multiplayer capabilities using Unity's Netcode for GameObjects and dedicated server infrastructure.

## Project Structure

### 🎮 Core Game Architecture

#### **Assets/1_Scripts/1_Runtime/** - Main Game Logic
- **1_ApplicationLifecycle/** - Application entry point and initialization
  - `ApplicationEntryPoint.cs` - Main application bootstrap, handles server/client role management
- **2_ConnectionManagement/** - Network connection handling
- **3_Core/** - Core game systems and utilities
- **4_Game/** - In-game mechanics and systems
  - `NetworkedGameState.cs` - Synchronized game state management
  - `GameEvents.cs` - Game-specific event system
  - `Characters/` - Player character implementations
  - `Controllers/` - Game controllers and input handling
  - `GameplayObjects/` - Interactive game objects
  - `Models/` - Game data models
  - `Views/` - Game UI and visual components
- **5_Metagame/** - Lobby, matchmaking, and menu systems
  - `MetagameApplication.cs` - Metagame application logic
  - `MetagameEvents.cs` - Metagame event system
  - `1_Models/` - Metagame data models
  - `2_Views/` - Menu and UI views
  - `3_Controllers/` - Metagame controllers
  - `4_Managers/` - Metagame managers
  - `12_CharacterSelect/` - Character selection system
- **6_Shared/** - Shared utilities and components
- **7_UnityGameServices/** - Unity Gaming Services integration

#### **Assets/1_Scripts/2_Utility/** - Utility Scripts
- `BugsBunnyLogger.cs` - Custom logging system
- `Helper.cs` - General utility functions

#### **Assets/1_Scripts/Editor/** - Editor Tools
- `BuildHelpers.cs` - Build automation utilities
- `BuildProcessor.cs` - Build process management

### 🎨 Content Assets

#### **Assets/2_Scenes/** - Game Scenes
- `BootStrap.unity` - Initial bootstrap scene
- `MetagameScene.unity` - Lobby and menu scene
- `GameScene01.unity` - Main game scene
- **Levels/** - Game levels
  - `Level 1.unity` through `Level 4.unity` - Individual game levels
  - `Asset Scene.unity` - Asset management scene

#### **Assets/3_Art/** - Art Assets
- **Praful_Art/** - Main art assets
  - **Free_Runner/** - Character and environment art
  - **FX Mega Pack/** - Visual effects
  - **MasterStylizedFX/** - Stylized effects
  - **VFX_Klaus/** - Additional visual effects

#### **Assets/4_Prefabs/** - Game Prefabs
- **1_MetaGame/** - Metagame prefabs
  - `CharacterSpawner.prefab` - Character spawning system
  - **Characters/** - Character prefabs
  - **Game/** - Game-specific prefabs
  - **Metagame/** - Menu and UI prefabs
- **2_GamePlay/** - Gameplay prefabs

#### **Assets/UI Toolkit/** - Modern UI System
- **1_LoginView/** - Login interface
- **2_MainMenuView/** - Main menu
- **3_UserProfileView/** - User profile management
- **5_LobbyView/** - Multiplayer lobby
- **6_ClientConnectingView/** - Connection status
- **7_GamePauseMenuView/** - In-game pause menu
- **DirectIPView/** - Direct IP connection
- **MatchRecapView/** - Post-match summary
- **MatchView/** - In-match UI

### 🔧 Configuration & Settings

#### **Assets/7_Settings/** - Project Settings
- **Build Profiles/** - Build configurations
  - `Android Client Profile.asset` - Android client build
  - `Linux Server Profile.asset` - Linux server build
- **PlayMode/** - Play mode configurations
  - `client.asset` - Client play mode settings
  - `Server.asset` - Server play mode settings

#### **Assets/AddressableAssetsData/** - Addressable Assets
- Asset grouping and management for dynamic loading

#### **Assets/InputSystem/** - Input System
- `FreeRunInputAction.inputactions` - Input action definitions
- `FreeRunInputAction.cs` - Input system integration

#### **Assets/Localization/** - Localization
- `English (en).asset` - English localization
- `Localization Settings.asset` - Localization configuration

### 🌐 Backend Infrastructure

#### **BackEnd/game_server_allocater/** - Server Allocation Service
- **Node.js Express server** for game server allocation
- **Dependencies:**
  - `express` - Web framework
  - `axios` - HTTP client
  - `dotenv` - Environment configuration
  - `uuid` - Unique identifier generation
- **Structure:**
  - `index.js` - Main server entry point
  - `routes/` - API route handlers
  - `services/` - Business logic services
  - `utils/` - Utility functions
  - `config/` - Configuration files

#### **BackEnd/ServerLayout/** - Server Deployment
- `layout.wlt` - Server layout configuration

### 📚 Documentation

#### **Documentation/my-game-docs/** - Project Documentation
- **Docusaurus-based documentation site**
- **Dependencies:**
  - `@docusaurus/core` - Documentation framework
  - `@docusaurus/preset-classic` - Classic theme
  - `react` - UI framework
- **Structure:**
  - `docs/` - Documentation pages
  - `blog/` - Development blog
  - `src/` - Custom components
  - `static/` - Static assets

### 🎯 Key Features

#### **Multiplayer Architecture**
- **Dedicated Server Support** - Linux server builds
- **Unity Netcode for GameObjects** - Network synchronization
- **Unity Gaming Services** - Authentication and matchmaking
- **Cloud Code Integration** - Server-side logic

#### **Game Features**
- **Character Selection** - Multiple playable characters
- **Level System** - Multiple game levels
- **Parkour Mechanics** - Movement and climbing system
- **Visual Effects** - Comprehensive VFX system
- **Localization** - Multi-language support

#### **Development Tools**
- **Custom Logging** - BugsBunnyLogger system
- **Build Automation** - Automated build processes
- **Test Runner** - Automated testing framework
- **Editor Tools** - Custom Unity editor extensions

### 🔌 Unity Packages

#### **Core Networking**
- `com.unity.netcode.gameobjects` - Netcode for GameObjects
- `com.unity.dedicated-server` - Dedicated server support
- `com.unity.transport` - Transport layer
- `com.unity.multiplayer.tools` - Multiplayer development tools

#### **Unity Services**
- `com.unity.services.multiplayer` - Multiplayer services
- `com.unity.services.cloudcode` - Cloud Code
- `com.unity.services.cloudsave` - Cloud save functionality
- `com.unity.services.deployment` - Deployment tools

#### **Graphics & Rendering**
- `com.unity.render-pipelines.universal` - Universal Render Pipeline
- `com.unity.cinemachine` - Camera system
- `com.unity.ai.navigation` - AI navigation

#### **Input & UI**
- `com.unity.inputsystem` - New Input System
- `com.unity.localization` - Localization system

### 🚀 Build & Deployment

#### **Build Profiles**
- **Android Client** - Mobile client build
- **Linux Server** - Dedicated server build
- **Windows Client** - Desktop client build

#### **Server Architecture**
1. **Player Authentication** via Unity Authentication
2. **Lobby System** for matchmaking
3. **Server Allocation** via custom Node.js service
4. **Dedicated Server** hosting game sessions
5. **Client Connection** to allocated servers

### 📁 Project Files

#### **Solution Structure**
- `FreeRun.sln` - Visual Studio solution
- `Assembly-CSharp.csproj` - Main game assembly
- `Assembly-CSharp-Editor.csproj` - Editor scripts
- `RedGaint.Network.Runtime.csproj` - Network runtime
- `RedGaint.Utility.csproj` - Utility library
- `RedGaint.Common.InputSystem.csproj` - Input system
- `Unity.Services.CloudCode.GeneratedBindings.csproj` - Cloud Code bindings

#### **Configuration Files**
- `.gitignore` - Git ignore rules
- `Packages/manifest.json` - Unity package dependencies
- `ProjectSettings/` - Unity project settings

---

## Development Workflow

### **Entry Point Flow**
1. **ApplicationEntryPoint** initializes based on role (Client/Server)
2. **Server** starts listening and loads game scene
3. **Client** loads metagame scene and connects to server
4. **ConnectionManager** handles network connections
5. **Game/Metagame Applications** manage respective game states

### **Multiplayer Flow**
1. Player signs in (Unity Authentication)
2. Player joins/creates lobby
3. When ready, lobby data sent to matchmaker
4. Matchmaker allocates game server
5. Server connection info returned to clients
6. All clients connect to game server
7. Game session begins

This project demonstrates a complete multiplayer game architecture with dedicated servers, modern Unity features, and comprehensive tooling for development and deployment. 
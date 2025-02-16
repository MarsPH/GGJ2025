# Flow  
*A physics-driven 2D game from Global Game Jam 2025*  
  

[![Game Jam](https://img.shields.io/badge/Game%20Jam-GGJ%202025-blue)](https://globalgamejam.org/)  
[![Unity](https://img.shields.io/badge/Made%20With-Unity-FF5733)](https://unity.com/)  

---  

## 🎮 Overview  

**Flow** is a 2D physics-driven game where players guide a soft-mesh bubble through hazardous obstacles using a dynamic line. The bubble deforms, shrinks, and expands to navigate sharp and narrow areas, making physics-based movement central to the gameplay.  

<p align="center">
  <img src="https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExYTlycndqMnI2bDhxenc2N21razM1a2ZnanR4cWhraG53cW41NHIyeiZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/XyVZphIeiupMdv0DWm/giphy.gif" alt="Flow Gameplay Preview">
</p>

---  

## 🧩 Key Components  

### 🟢 Bubble.cs  
- Core logic for the bubble character.  
- Handles collisions, absorption (other bubbles & food), and soft-mesh deformation.  
- Implements stress and damage mechanics.  

### ✨ Collectible.cs  
- Manages pickups and plays sound effects upon collection.  

### 🛑 PlayerRespawn.cs  
- Implements checkpoint-based respawning for the player bubble.  

### 📊 GameManager.cs  
- Oversees game state, collectible count, heart (lives) management.  
- Controls scene transitions (win/lose).  

### 💥 BubbleMovement.cs  
- Processes player input for charge-based movement.  

---  

## ⚙️ Usage Examples  

📌 **Customize Bubble Physics**  
- Adjust **Blob.cs** parameters like deformation thresholds and spring settings to tweak movement.  

📌 **Game Flow Control**  
- Use **GameManager.cs** for UI updates and state transitions.  

📌 **Fine-Tune Player Control**  
- Experiment with **BubbleMovement.cs** explosion force to modify responsiveness.  

---  

## 📌 Installation & Running the Game  

```bash
git clone https://github.com/your-username/flow.git
cd flow
```
Run in Unity **2022.x or newer**.

---  

## 🏆 Contributors  
- **[Mahan](https://github.com/MarsPH)** – Developer  
 

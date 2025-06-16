# 🍋 Citric Acid Allergy

**Citric Acid Allergy** is a game developed for the **Thatgamecompany × COREBLAZER GAME JAM 2025**, created using **Unity 6000.0.26f1**. This experimental game combines cooperation with players powered by serverless AWS infrastructure.

---

## 🎮 About the Game

In the far reaches of the cosmos, our species shares one universal weakness: lemon juice. It’s our ultimate nemesis—our kryptonite.

But now, hordes of cosmic lemons are closing in, determined to block our path home! Band together, strategize, and fight back against the acidic threat. Only by working as one can we defeat the evil space lemons and reclaim our rightful place among the stars!

Do you have what it takes to survive the citrus onslaught?

---

## 🧠 Tech Stack

- **Engine**: Unity 6000.0.26f1  
- **Backend**: AWS Lambda (serverless functions)  
- **Database**: Amazon DynamoDB  
- **Languages**: C# (Unity), Python (Lambda)

---

## 🖼️ Art Assets

Huge thanks to the following artists for providing their assets:

- [Tilemap SF by oshq](https://oshq.itch.io/tilemap-sf)  
- [Ocunid Monster Sprite by robocelot](https://robocelot.itch.io/ocunid-monster-sprite)  
- [Free Food Icons by azuna-pixels](https://azuna-pixels.itch.io/free-food-icons)  
- [FX062 by nyknck](https://nyknck.itch.io/fx062)  

---

## ☁️ Backend Setup Guide

To run the AWS serverless backend used in **Citric Acid Allergy**, follow these steps:

### 1. DynamoDB Setup

Create a DynamoDB table with the following configuration:

- **Table Name**: `GameJamReinforcement`  
- **Partition Key**: `uuid` (String)  
- **Global Secondary Index (GSI)**:
  - **Index Name**: (e.g., `level-value-index`)
  - **Partition Key**: `level`
  - **Sort Key**: `value`

### 2. AWS Lambda Functions

Create two Lambda functions:

- `SubmitReinforcement`  
- `GetReinforcement`

> 🧠 Tip: Use **AWS API Gateway** to expose your Lambda functions over HTTPS (recommended).

### 3. Deploy Python Scripts

- Paste the provided Python scripts into their respective Lambda functions.
- Ensure the functions have appropriate IAM roles to access DynamoDB.

### 4. Unity Configuration

In your Unity project:

- Open the `LambdaManager.cs` script.
- Replace the placeholder URLs with your deployed Lambda or API Gateway endpoint URLs.

---

## 🛠️ Development Notes

- Uses Unity’s coroutine system for asynchronous HTTP requests.
- No third-party Unity plugins required.
- Gameplay balance subtly adapts to user behavior via server-driven reinforcement.

---

## 📜 License

This project was developed for educational and creative purposes as part of a game jam. Please respect the MIT licenses and terms of use of all third-party assets included in this project.

---

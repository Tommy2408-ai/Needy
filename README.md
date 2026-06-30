# 🤝 Needy 

**A community-driven, Open Source mobile application for neighborhood mutual aid.**  
*Developed for the Youth Hacking 4 Freedom (YH4F) 2026 competition.*

---

## 🌍 The Problem & The Solution
Many people (the elderly, busy families, or people with disabilities) occasionally need minor everyday help. On the other hand, many neighbors have free time but no safe and easy channel to offer it.  

**Needy** digitalizes the "Time Bank" concept. It is a secure, geo-localized, and self-hostable platform where users can request help (e.g., grocery shopping, companionship, small repairs) and volunteers can offer their time, earning "Reputation Hours" in return.

## ✨ Key Features
- **Two-Step Approval System:** Admin verification for new accounts using ID documents (Security by design).
- **Time Banking Economy:** Volunteers earn reputation hours automatically when a task is marked as "Completed".
- **Smart Dashboard:** Users can track their own requests and the ones they are contributing to.
- **In-App Notifications:** Real-time application updates for candidates offering help.
- **Privacy-First:** Open Source and ready to be self-hosted by local municipalities or NGOs.

## 📸 Screenshots & Architecture

### Application Architecture
<img width="7041" height="1465" alt="Diagramma dei Casi d&#39;Uso" src="https://github.com/user-attachments/assets/5c273b4b-0be9-45ed-9c14-0da74a2bd70f" />

<img width="2300" height="2338" alt="Diagramma delle Classi" src="https://github.com/user-attachments/assets/f2063e32-bb48-42d6-967c-e9a803b285e9" />

### App Interface
*(Drag and drop a screenshot of the Home/Community Board here)*
*(Drag and drop a screenshot of the Profile Page here)*

## 🛠️ Tech Stack
- **Frontend:** C# / .NET 10 MAUI (Cross-platform: Android, iOS, Windows)
- **Backend & Database:** PocketBase (Go/SQLite)
- **Data Transfer:** JSON (using Newtonsoft.Json)
- **Architecture Pattern:** MVVM-inspired with Dependency Injection.

## 🚀 How to Run the Project (For the Jury)

### 1. Backend Setup (PocketBase)
1. Download [PocketBase](https://pocketbase.io/docs/) for your OS.
2. Place the executable in the `Backend` folder.
3. Open a terminal in the `Backend` folder and run:
   ```bash
   ./pocketbase serve --http="0.0.0.0:8090"
1. Go to http://127.0.0.1:8090/_/ and set up your admin account.
2. Go to **Settings > Import collections**, and load the `pb_schema.json` provided in the Backend folder.

### 2. App Setup
- **Option A (Quick Install):** Download the `Needy-Signed.apk` file from this repository and install it directly on an Android device/emulator.
- **Option B (From Source)**
    1. Open `Needy.slnx` in Visual Studio 2026.
    2. In `MauiProgram.cs`, ensure the PocketBase IP matches your local network IP (e.g., `192.168.1.X:8090` or `10.0.2.2:8090` for the emulator).
    3. Build and Run!

 ## 🔮 Future Developments
 If development continues, the next features will be:
 - **In-App Chat:** Real-time messaging between requester and helper.
 - **Interactive Map:** Integration with OpenStreetMap to view requests geographically.
 - **Rating System:** A 5-star rating system after request completion.

## 📄 License
This project is licensed under the **GNU General Public License v3.0**. See the `LICENSE` file for details.

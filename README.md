# E-Learning Platform

A full-stack educational application designed to provide an interactive and gamified learning experience for primary school students.

The project was developed as my Bachelor's thesis in Economic Informatics and combines a **Unity client** with a **Spring Boot REST API** and an **Oracle database**.

The platform includes interactive Mathematics and English lessons, user authentication, progress tracking, virtual rewards, achievements, profile customization, and a virtual shop.

## Architecture

The application follows a client-server architecture:

```text
┌─────────────────────────┐
│      Unity Client       │
│        C# / Unity       │
└────────────┬────────────┘
             │
             │ REST API / JSON
             │ JWT Authentication
             ▼
┌─────────────────────────┐
│   Spring Boot Backend   │
│          Java           │
└────────────┬────────────┘
             │
             │ JPA / Hibernate
             ▼
┌─────────────────────────┐
│     Oracle Database     │
└─────────────────────────┘
```

The Unity application handles the user interface, lessons, gamification features, and player interaction, while the Spring Boot backend manages authentication, application data, business logic, and database access.

## Tech Stack

### Client

* Unity
* C#
* Unity UI
* Text-to-Speech integration
* Speech-to-Text integration

### Backend

* Java
* Spring Boot
* Spring Web / REST
* Spring Security
* Spring Data JPA
* Hibernate
* JWT authentication
* Maven

### Database

* Oracle Database
* Oracle Autonomous Database

### Development Tools

* Git & GitHub
* JetBrains Rider
* IntelliJ IDEA
* Postman

## Features

### User Authentication

The application provides an account-based authentication system backed by the Spring Boot API.

* User registration
* User login
* Secure password hashing
* JWT-based authentication
* User session management

### Interactive Learning

The platform contains educational activities for Mathematics and English.

**Mathematics**

* Addition and subtraction
* Multiplication and division
* Numbers
* Measurements
* Reading the clock
* Geometric shapes

**English**

* Synonyms
* Antonyms
* Sentence completion
* Syllables
* Pronunciation
* Dictation
* Reading activities

### Gamification

Learning activities are combined with several gamification systems designed to encourage progression and engagement.

* Experience points
* Player levels
* Virtual coins
* Achievements and badges
* Progress tracking
* Daily rewards
* Unlockable content

### Virtual Shop

Players can use earned coins to customize their experience and purchase bonuses.

Available items include:

* Profile avatars
* Backgrounds
* Double XP boosters
* Double coin boosters

### Player Profile

Each user has an individual profile containing information such as:

* Level
* Experience
* Coins
* Learning progress
* Achievements
* Owned customization items
* Active boosters

## Project Structure

```text
e-learning-platform/
│
├── unity-client/
│   ├── Assets/
│   ├── Packages/
│   ├── ProjectSettings/
│   └── ...
│
├── backend/
│   ├── src/
│   ├── pom.xml
│   └── ...
│
├── .gitignore
└── README.md
```

### `unity-client`

Contains the Unity application responsible for the graphical interface, lessons, gameplay logic, profile customization, and gamification systems.

### `backend`

Contains the Spring Boot application responsible for REST endpoints, authentication, business logic, persistence, and communication with the Oracle database.

## Authentication Flow

```text
User
 │
 │ Credentials
 ▼
Unity Client
 │
 │ POST / authentication request
 ▼
Spring Boot API
 │
 ├── Validate credentials
 ├── Verify hashed password
 └── Generate JWT
 │
 ▼
Unity Client
 │
 │ JWT
 ▼
Authenticated API Requests
```

Passwords are stored securely using password hashing, while JWT tokens are used to authenticate subsequent API requests.

## Getting Started

### Prerequisites

To run the complete project locally, you will need:

* Unity 6
* Java
* Maven
* Oracle Database or access to an Oracle database instance
* Git

### Clone the Repository

```bash
git clone https://github.com/antonescualex/e-learning-platform.git
cd e-learning-platform
```

### Backend

Navigate to the backend:

```bash
cd backend
```

Configure the required database connection, OpenAI API key and application settings.

Then run the Spring Boot application:

```bash
./mvnw spring-boot:run
```

On Windows:

```bash
mvnw.cmd spring-boot:run
```

### Unity Client

Open Unity Hub and add the following directory as an existing project:

```text
unity-client/
```

Open the project using the compatible Unity version and start the application from the appropriate initial scene.

## Development

This project was developed as my Bachelor's thesis at the **Bucharest University of Economic Studies (ASE)**, Faculty of Cybernetics, Statistics and Economic Informatics.

The project provided practical experience with:

* Full-stack application architecture
* REST API development
* Client-server communication
* Authentication and authorization
* Relational database persistence
* Object-relational mapping
* Unity application development
* Gamification systems
* Git version control

## Future Improvements

Potential improvements include:

* Automated backend testing
* Additional educational modules
* Improved analytics for learning progress
* Teacher and administrator roles
* Expanded achievement system
* Deployment of the backend and database to a production environment
* CI/CD pipeline

## Author

**Alex Antonescu**

B.Sc. Economic Informatics
M.Sc. IT&C Security — in progress

GitHub: [@antonescualex](https://github.com/antonescualex)

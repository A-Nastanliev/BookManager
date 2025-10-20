# 📚 Book Manager – Mobile Application

**Book Manager** is a mobile application for tracking readers' progress. It allows users to manage their reading activity and view statistics based on their interests and achievements.

## 🔧 Project Overview

The Book Manager project is structured into three main layers:
- **Data Layer**
- **Service Layer**
- **Presentation Layer**

The application is intended for users who want to monitor their reading progress, interact with books, and receive statistical insights about their performance.

---

## 👤 Users

### 1. Regular User

After logging in, each user has access to the following features:

- Possesses a username, password, and email
- Can search for books by:
  - Genre
  - Author
  - Publisher
  - Other criteria
- Publishes comments on books
- Enters and marks the number of pages read
- Adds books to a wish list

### 2. Administrator

Initially, there is one administrator in the system, who can:

- Create, edit, search, and delete users
- Add, edit, search, and delete books
- Manage user-submitted book suggestions (approve/reject)

---

## 📊 Statistics

The application provides statistical data related to reading, including:

- Pages read
- Total number of books
- Number of partially read books

Statistics are displayed for the following time periods:

- Weekly
- Monthly
- Yearly
- Entire usage period

---

## 🛠️ Project Structure

- `data-layer/` – handles data management and storage
- `service-layer/` – contains business logic and request handling
- `presentation-layer/` – manages the user interface

---

## 🖥 Technologies

- MAUI
- EntityFramework
- Jira

---

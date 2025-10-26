# 📚 Book Manager – Mobile Application

**Book Manager** is a mobile application for tracking readers' progress. It allows users to manage their reading activity and view statistics based on their interests and achievements.

## 🔧 Project Overview

The Book Manager project structure:
- **Data Layer**
- **Business Layer**
- **Testing Layer**
- **Service Layer**
- **Presentation Layer**

The application is intended for users who want to monitor their reading progress, interact with books, and receive statistical insights about their performance.

---

## 👤 Users

### 1. Regular User

After logging in, each user has access to the following features:

- Possesses a username, password and email
- Can search for books by:
  - Genre
  - Author
  - Publisher
  - Other criteria
- Publishes comments on books or rates them
- Enters and marks the number of pages read
- Adds books to a wishlist
- Submits requests for new books to be added to the system

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

## 🖥 Technologies

- MAUI
- EntityFramework

---

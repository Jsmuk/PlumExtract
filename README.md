# 🍑 Plum Extract  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  

---

## 📘 What  
Simple tool to extract data from [Plum](https://withplum.com/) statements into various formats.

---

## 🤔 Why  
I like Plum, I like graphs where line go up. I don't like having my statements only as PDFs and not being able to do what I want with them.

---

## ⚙️ How  
**TODO**

---

## 🗂 Project Structure

### 🔌 Adding a New Storage Provider

- Create a project named `PlumExtract.Storage.AmazingStorageSystem`
- Implement `IBlobStore`
- Add a `<ProjectReference>` in `PlumExtract.csproj` with:
  ```xml
    <ProjectReference Include="..\PlumExtract.Storage.AmazingStorageSystem\PlumExtract.Storage.AmazingStorageSystem.csproj"
                      ReferenceOutputAssembly="false"
                      OutputItemType="StoragePluginDll"
                      CopyLocal="true" />
  ```
- The plguin will build and be copied to `StorageProviders` for runtime discovery 
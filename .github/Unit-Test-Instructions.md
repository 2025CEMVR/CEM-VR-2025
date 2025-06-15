# Running Unit Tests in Unity

This project uses Unity's built-in Test Framework.

## How to Run Tests

1. Open the Unity project in the Unity Editor.
2. Go to **Window > General > Test Runner**.
3. In the Test Runner window:
   - Select **Edit Mode** for logic-only tests.
   - Select **Play Mode** for scene-based tests.
4. Click **Run All** to execute all available tests.

## Test Folder Structure

Place your tests in one of the following directories:

- `Assets/Tests/Editor` — for Edit Mode tests
- `Assets/Tests/PlayMode` — for Play Mode tests

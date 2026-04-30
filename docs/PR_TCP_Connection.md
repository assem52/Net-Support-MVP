# Pull Request: TCP Connection (Remote Lock/Unlock)

## Overview
This PR introduces **Phase 2: Remote Lock / Unlock**, replacing the purely UDP-based discovery with reliable TCP connections for critical commands. The Tutor can now select a specific student from the dashboard and lock their screen, preventing any input or circumvention, and unlock it when ready.

## Logic & Implementation

### 1. The Need for TCP
While UDP was perfect for broadcasting presence ("I am here!"), it doesn't guarantee delivery. A `LOCK` command is critical; if it gets lost in the network, the student's PC would remain unlocked during an exam. Therefore, we use **TCP** to establish a direct, guaranteed connection to the specific student's IP address.

### 2. Student Side (`NetSupport.Student`)
- **`TcpCommandListener`**: We added a background service that listens on Port `9001` using `TcpListener`. When a connection is accepted, it reads the JSON stream. If the type is `LOCK` or `UNLOCK`, it triggers the respective C# events.
- **`LockScreenWindow`**: A new borderless WPF Window (`WindowStyle="None"`) was created. 
  - `WindowState="Maximized"` ensures it covers the screen.
  - `Topmost="True"` ensures it stays aggressively on top of all other applications.
  - We overrode `OnPreviewKeyDown` to intercept and cancel `Alt-F4`, preventing the student from bypassing the lock.
- **`App.xaml.cs`**: Subscribes to the listener events and uses `Current.Dispatcher.Invoke` to safely marshal the window creation/destruction back to the main UI thread.

### 3. Tutor Side (`NetSupport.Tutor`)
- **`TcpCommandSender`**: A helper class that uses `TcpClient` to connect to the target IP, serializes the `LOCK` or `UNLOCK` `NetworkMessage` to JSON, writes it to the stream, and immediately disconnects.
- **Dashboard UI**:
  - Added "Lock Selected" and "Unlock Selected" buttons above the grid.
  - Added an `IsLocked` boolean property to `StudentHelloPayload`.
  - Implemented `INotifyPropertyChanged` on the payload so the DataGrid automatically updates to show a checkmark in the `IsLocked` column when the Tutor clicks the buttons.

## How to Test This PR
1. Build the solution and run `NetSupport.Tutor`.
2. Run `NetSupport.Student` on the same or another PC on the LAN.
3. Wait for the student to appear in the Tutor dashboard (via the existing UDP discovery).
4. Click the row to select the student.
5. Click **"Lock Selected"**. The student's screen will instantly show the lock overlay, and the Tutor dashboard will check the `IsLocked` box.
6. Click **"Unlock Selected"**. The lock overlay will disappear, and the checkbox will uncheck.

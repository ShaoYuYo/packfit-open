# PackFit Engineering: 無法啟動的排查指南

這份說明適用於 **v0.7.0-beta.4 及後續版本**。beta.1、beta.2 並沒有這些啟動診斷工具。以下步驟不會自動安裝其他軟體，也不需要管理員權限；如果公司的資安政策不允許執行，請先詢問 IT。

## 先確認下載了哪個版本

**推薦：完整 ZIP 版**。從 GitHub Release 下載 `PackFit-Engineering-win-x64.zip`，在檔案總管選擇「全部解壓縮」，放到一般本機資料夾（例如 Downloads 或 Documents）。請勿只從 ZIP 裡直接執行 EXE，也不要單獨複製 `PackFit.Engineering.exe`。原生 DLL、WinUI 資源與 .NET runtime 是整套程式的一部分。

**單檔版不是第一個排查選項。** WinUI 的單檔 EXE 可能會在首次啟動時解壓 DLL 到暫存目錄；企業限制、磁碟空間或防護軟體可能影響啟動。若單檔版失敗，先改試完整 ZIP 版。

本版本支援 Windows 10（build 19041+）與 Windows 11 的 x64 環境。並非所有 Windows ARM、舊系統或企業管理裝置都經過測試。

## 快速排查：三個步驟

1. 進入已解壓縮的資料夾，雙擊 **`Start-PackFit.cmd`**。它會從正確的工作目錄啟動 EXE，並保留命令視窗，方便看到返回碼及本機診斷路徑。
2. 如果介面閃退或沒有出現，請在該資料夾按 Shift + 右鍵，選擇「在終端機中開啟」，輸入：
   ```powershell
   .\PackFit.Engineering.exe --safe-ui
   ```
   Beta 4 的安全模式**完全不會載入 MainWindow.xaml**，而是開啟另一個以 C# 建立的相容工程介面，具備電池報告、CSV/TSV、驗證矩陣、基本 Pack 估算與 Markdown 報告功能。若能開啟，至少可以繼續工作並確認問題集中於主介面載入流程；還不能直接判定是哪個 Fluent 元素造成的。
3. 再次失敗時，檢查這個本機檔案：
   ```text
   %LOCALAPPDATA%\PackFitOpen\Logs\startup.log
   ```
   可將上面的路徑貼到 Win + R 或檔案總管的網址列。若看不到此檔案，故障可能發生在 .NET / WinUI 程式的託管入口之前，此時需要 Windows 的事件檢視器。

## 一鍵收集本機診斷

在解壓縮後的資料夾，以 PowerShell 執行：

```powershell
.\Diagnostics\Diagnose-PackFit.ps1
```

如果要用簡化介面試啟動：

```powershell
.\Diagnostics\Diagnose-PackFit.ps1 -SafeUi
```

PowerShell 腳本會檢查 EXE 是否存在、旁邊是否有相依 DLL、啟動後六秒內是否已退出，並在允許的情況下讀取 Windows Application 事件記錄。結果只寫在本機：

```text
%LOCALAPPDATA%\PackFitOpen\Logs\launch-check.txt
```

**如果公司禁止執行 PowerShell 腳本，請不要繞過執行原則。** 改用上面的 CMD 啟動器與 Windows 事件檢視器。

### 如果完全沒有 startup.log

通常代表程式沒有進入 PackFit 的 C# 啟動程式，可能是本機缺少必要元件、檔案未完整解壓、原生 Windows App SDK 載入失敗，或防護軟體阻擋。這些只是可能原因，**必須依事件記錄來判斷**。

1. 按 Win + R，輸入 `eventvwr.msc`。
2. 進入「Windows 記錄 → 應用程式」。
3. 找剛剛啟動時間附近的 **Application Error** 或 **.NET Runtime** 訊息。
4. 記下「Faulting application」、「Faulting module」和「Exception code」的內容。不要分享裝置序號、使用者帳號或內部路徑。

| 現象 | 下一步 |
|---|---|
| 只有單檔 EXE 閃退 | 改用完整 ZIP。單檔會解壓暫存相依檔案 |
| EXE 雙擊沒反應，沒有 startup.log | 看 Windows Event Viewer 的故障模組與例外碼 |
| 有 startup.log，MainWindow 初始化失敗 | 開啟 recovery 視窗或 `--safe-ui`，記下錯誤摘要 |
| Windows SmartScreen 或公司政策封鎖 | 驗證官方 Release 與雜湊，向 IT 詢問；**不要繞過安全限制** |
| 只解壓出一個 EXE，沒有其他 DLL | 重新完整解壓 ZIP 版 |
| `0xc000027b`，涉及 `Microsoft.UI.Xaml.dll` | 這是 Windows App SDK 的「stowed exception」包裝碼，需提供事件詳細資訊；不能單靠碼判定問題 |

## 已確認的打包問題：EXE 重新命名

Beta 1–3 的單檔程式在編譯後被重新命名成 `PackFit-Engineering-portable-win-x64.exe`。微軟曾有 [WinUI 問題回報](https://github.com/microsoft/microsoft-ui-xaml/issues/10966) 指出，重新命名 self-contained EXE 後可能產生 XAML parsing failed。**Beta 4 已改為直接發布原始檔名 `PackFit.Engineering.exe`，請不要自行更名。** 但尚未確認這是否就是原本電腦上的根因。

若你是使用 Beta 3 的單檔 EXE，優先改試 Beta 4 的 `PackFit.Engineering.exe`，或完整 ZIP。如果問題仍存在，請提供新版 `startup.log`，其中會記錄實際執行檔名稱、Windows 版本及 XAML HRESULT，幫助定位真正原因。

## 完整版與單檔版的技術差異

Microsoft 官方文件說明，unpackaged WinUI 3 以 `WindowsAppSDKSelfContained=true` 和 `WindowsPackageType=None` 部署時，完整資料夾可攜帶 Windows App SDK 依賴。若使用 `PublishSingleFile=true`，必須符合專用 MSBuild 配置，且首次啟動會把原生相依元件解壓到暫存資料夾。完整資料夾通常更適合工程試用。

- 官方文件：https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/unpackage-winui-app
- 官方 Fluent 架構：https://learn.microsoft.com/en-us/windows/apps/develop/ui/windows-app-sdk-app-structure
- GitHub 排錯 Issue：https://github.com/ShaoYuYo/packfit-open/issues

## 資料隱私

上述診斷只在本機執行，**不會傳送任何資訊到 OpenAI、GitHub 或開發者**。但 Windows 事件、C# Exception 可能含有使用者資料夾名稱或私有路徑。提交公開 Issue 前，請自行刪除敏感資訊。

此版本提供改善後的故障診斷與安全模式，並非保證所有 Windows 系統一定可啟動。通過 GitHub Actions 的編譯，不代表已在您的 Windows 裝置上實機驗證。

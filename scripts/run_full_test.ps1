# Full end-to-end test script for JarApi
# Usage: run in PowerShell. This script will:
# - start the app on http://localhost:5257
# - wait until the app responds
# - register a user and login
# - create a role, company, menu, widget
# - assign menu/widget to role
# - assign role to user in company
# - fetch user-permissions and print full JSON
# - stop the app process

param()

$root = "C:\Users\mh.derakhshesh\Desktop\JarApi"
$url = 'http://localhost:5257'

Write-Output "Starting JarApi (dotnet run) in $root..."
$proc = Start-Process -FilePath 'dotnet' -ArgumentList 'run --urls http://localhost:5257' -WorkingDirectory $root -PassThru

# Wait for app to be ready
$ready = $false
for ($i=0; $i -lt 30; $i++) {
    try {
        $r = Invoke-WebRequest -Uri "$url/swagger/index.html" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        $ready = $true
        break
    } catch {
        Start-Sleep -Seconds 1
    }
}

if (-not $ready) {
    Write-Error "App did not start within timeout. Check console for errors."
    exit 1
}

Write-Output "App is ready. Running tests..."

# Helper to POST and return parsed JSON
function Post($endpoint, $body, $token=$null) {
    $headers = @{}
    if ($token) { $headers['Authorization'] = "Bearer $token" }
    $json = $body | ConvertTo-Json -Depth 10
    return Invoke-RestMethod -Method Post -Uri "$url$endpoint" -Body $json -ContentType 'application/json' -Headers $headers -ErrorAction Stop
}

function GetJson($endpoint, $token=$null) {
    $headers = @{}
    if ($token) { $headers['Authorization'] = "Bearer $token" }
    return Invoke-RestMethod -Method Get -Uri "$url$endpoint" -Headers $headers -ErrorAction Stop
}

# 1) Register user
$user = @{ PersonnelCode = 'auto_user'; Password = 'P@ssw0rd!'; FirstName='Auto'; LastName='Tester' }
Write-Output "Registering user..."
$reg = Post '/api/auth/register' $user
Write-Output "Register response:"; $reg | ConvertTo-Json -Depth 5
$token = $reg.Token

# 2) Create role
$roleName = 'AutoRole'
Write-Output "Creating role $roleName..."
try {
    $roleRes = Post '/api/role' @{ RoleName = $roleName } $token
    Write-Output "Create role response:"; $roleRes | ConvertTo-Json -Depth 5
} catch {
    Write-Warning "Create role failed (might exist). Continuing. $_"
}

# 3) Get roles
$roles = GetJson '/api/role' $token
Write-Output "Roles:"; $roles | ConvertTo-Json -Depth 5
$roleId = $roles | Where-Object { $_.name -eq $roleName } | Select-Object -First 1 -ExpandProperty id
if (-not $roleId) { $roleId = $roles[0].id }

# 4) Create company
Write-Output "Creating company..."
$company = Post '/api/company' @{ Name='AutoCo'; Code='AUTO' } $token
Write-Output "Company:"; $company | ConvertTo-Json -Depth 5
$companyId = $company.Id

# 5) Create menu
Write-Output "Creating menu..."
$menu = Post '/api/menu' @{ Name='Dashboard'; Title='داشبورد'; Url='/dashboard'; IsActive = $true; DisplayOrder=1 } $token
Write-Output "Menu:"; $menu | ConvertTo-Json -Depth 5
$menuId = $menu.Id

# 6) Create widget
Write-Output "Creating widget..."
$widget = Post '/api/widget' @{ Name='SampleWidget'; Title='نمونه ویجت'; WidgetType='Chart'; IsActive=$true; DisplayOrder=1 } $token
Write-Output "Widget:"; $widget | ConvertTo-Json -Depth 5
$widgetId = $widget.Id

# 7) Assign menu and widget to role
Write-Output "Assigning menu/widget to role..."
$assignMenus = @{ RoleId = $roleId; Menus = @(@{ MenuId = $menuId; CanView = $true; CanCreate = $false; CanEdit = $false; CanDelete = $false }) }
$assignWidgets = @{ RoleId = $roleId; Widgets = @(@{ WidgetId = $widgetId; CanView = $true; CanConfigure = $false }) }
Post '/api/role/assign-menus' $assignMenus $token | Out-Null
Post '/api/role/assign-widgets' $assignWidgets $token | Out-Null
Write-Output "Assigned menus and widgets."

# 8) Assign role to user in company
Write-Output "Assigning role to user in company..."
Post '/api/role/assign-role-to-user-in-company' @{ PersonnelCode='auto_user'; RoleId = $roleId; CompanyId = $companyId } $token | Out-Null
Write-Output "Assigned role to user in company."

# 9) Get user permissions
Write-Output "Fetching user-permissions..."
$perms = GetJson '/api/auth/user-permissions' $token
Write-Output "User Permissions:"; $perms | ConvertTo-Json -Depth 10

# 10) Stop app
Write-Output "Stopping app (killing dotnet processes)..."
Get-Process dotnet -ErrorAction SilentlyContinue | Where-Object { $_.Path -like '*JarApi*' -or $_.StartInfo.WorkingDirectory -eq $root } | ForEach-Object { Write-Output "Stopping process Id=$($_.Id)"; Stop-Process -Id $_.Id -Force }
Write-Output "Done"

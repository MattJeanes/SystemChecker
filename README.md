# SystemChecker
Scalable system to check other systems

[![CircleCI](https://img.shields.io/circleci/project/github/MattJeanes/SystemChecker.svg)](https://circleci.com/gh/MattJeanes/SystemChecker)

#### SystemChecker.Web
[![Docker Stars - SystemChecker.Web](https://img.shields.io/docker/stars/mattjeanes/systemchecker.web.svg)](https://hub.docker.com/r/mattjeanes/systemchecker.web/)
[![Docker Pulls - SystemChecker.Web](https://img.shields.io/docker/pulls/mattjeanes/systemchecker.web.svg)](https://hub.docker.com/r/mattjeanes/systemchecker.web/)
#### SystemChecker.Service
[![Docker Stars - SystemChecker.Service](https://img.shields.io/docker/stars/mattjeanes/systemchecker.service.svg)](https://hub.docker.com/r/mattjeanes/systemchecker.service/)
[![Docker Pulls - SystemChecker.Service](https://img.shields.io/docker/pulls/mattjeanes/systemchecker.service.svg)](https://hub.docker.com/r/mattjeanes/systemchecker.service/)
#### SystemChecker.Migrations
[![Docker Stars - SystemChecker.Migrations](https://img.shields.io/docker/stars/mattjeanes/systemchecker.migrations.svg)](https://hub.docker.com/r/mattjeanes/systemchecker.migrations/)
[![Docker Pulls - SystemChecker.Migrations](https://img.shields.io/docker/pulls/mattjeanes/systemchecker.migrations.svg)](https://hub.docker.com/r/mattjeanes/systemchecker.migrations/)

## Features
- Scalable - multiple instances of frontend and backend can run simultaneously to split the load
- Runs "checks", e.g. check if google.com responds within 2000ms and the response contains the word "Google"
- Different types of checks
  - HTTP Check with optional authentication
  - Database Check
  - Ping Check
- Sub-checks, e.g. check if response contains 'Hello' or SQL result is greater than 5
- Schedules using cron expressions, e.g. run every weekday every hour between 6am-8pm
- Notifications, e.g. if check fails for 5 minutes send slack message to #failures
- Different notification types
  - Slack
  - Email
  - SMS
- Dashboard
  - Shows a summary of all checks with pie charts on which are succeeding/failing/warning
  - Easily filterable by clicking on the pie chart or by using the filters on the table
  - Can goto the results view by clicking on it, or press the edit/copy/run buttons
- Results view ("details")
  - Shows in a graph all of the results for a particular check between configurable data ranges
  - Easily filterable to show only successes/warnings/failures by clicking on the legend
  - Can be zoomed by using the bar along the bottom
- Manual UI run
  - Allows any check to be run manually from the UI from the dashboard, results or edit pages
  - Shows a detailed log on exactly how the check is running
  - Useful for debugging the check itself or seeing what is going wrong to fix it in your system
- User configuration
  - Optional Windows Authentication within a certain Active Directory group
  - Basic username/password users (you'll be asked to create one if there aren't any for initial setup)
  - Configurable API keys (per user)
- Settings
  - Global settings
    - SMTP settings for email notifications
    - Clickatell API settings for SMS notifications
    - Slack token for slack notifications
    - Windows Authentication group for automatic login
    - Schedules and settings for automatic data cleanup/aggregation
    - Login expiration setting
    - Time zone setting for the schedules, UTC by default
  - Logins, used by HTTP Check type to authenticate with a website
  - Connection Strings, used by Database Check type to login to a SQL Server
  - Environments, used to split up checks on the dashboard, e.g. Production/Test
  - Contacts, used by email/SMS notifications to choose who to send to
  - Check groups, used to group up checks into system e.g. MySystem - Front Page

# Setup

Environment variables:
- `ConnectionStrings__SystemChecker`
  - Example: `Database=SystemChecker;Data Source=sql.yoursite.com;User Id=systemchecker;Password=test`
  - Used for database connectivity
- `AppSettings__Url`
  - Example: `https://systemchecker.yoursite.com`
  - Used by notifications to include a link back to the check
- `RedisUrl`
  - Example: `redis:6379` or `localhost:6379`
  - Used for communication between backend (service) and frontend (web), must be the same between everything
- `ASPNETCORE_URLS`
  - Example: `localhost:5000`
  - Used by ASP.NET Core to bind the frontend site to
  
## First-time setup
Before first start, create a new empty database and create a user with read/write permissions and configure the `ConnectionStrings__SystemChecker` environment variable accordingly.

Make sure a redis server is running and configure the `RedisUrl` environment variable accordingly.

Finally, make sure the `AppSettings__Url` is set to the reachable address of where you plan to host the site. 

## Database Migrations (SystemChecker.Migrations)
On first-start or when upgrading, run the migrations tool with an appropriate `ConnectionStrings__SystemChecker` environment variable with enough permissions to modify the database schema or the migrations will fail.

## Frontend (SystemChecker.Web)
Start the program with the appropriate `ConnectionStrings__SystemChecker`, `AppSettings__Url`, `RedisUrl` and `ASPNETCORE_URLS` environment variables, depending on your deployment you may have a Windows executable or you may need to call `dotnet SystemChecker.Web.dll`

## Backend (SystemChecker.Service)
Start the program with the appropriate `ConnectionStrings__SystemChecker`, `AppSettings__Url` and `RedisUrl` environment variables, as above you may have a Windows executable or you may need to call `dotnet SystemChecker.Service.dll`

A command line option `-s|--service` is available which tells the program to run as a Windows service, useful for Windows-based deployments

A help command line option `-?|-h|--help` is also available.

# Screenshots

## Dashboard
![Dashboard](https://i.imgur.com/dcId0ZR.png)

## Edit Check
![Edit Check 1](https://i.imgur.com/GxtfhTb.png)

![Edit Check 2](https://i.imgur.com/dSPs8kP.png)

![Edit Check 3](https://i.imgur.com/XuM3IFf.png)

![Edit Check 4](https://i.imgur.com/gYzgeWW.png)

## Details
![Details](https://i.imgur.com/bxwzhet.png)

## Settings
![Settings 1](https://i.imgur.com/7RSTPm0.png)

![Settings 2](https://i.imgur.com/fo5ViHG.png)

![Settings 3](https://i.imgur.com/aLTeUux.png)

![Settings 4](https://i.imgur.com/NfE9BJr.png)

![Settings 5](https://i.imgur.com/qvJtGvq.png)

![Settings 6](https://i.imgur.com/oUy3xzx.png)

## User
![Login](https://i.imgur.com/QCE7Ji5.png)
![User](https://i.imgur.com/RQ0ICUD.png)

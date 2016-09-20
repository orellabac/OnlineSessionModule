

Online Users Module
===================

A simple module that you can add to your website to monitor current users and session sizes

Usage Instructions
==================

NOTE: We assume that you have an ignore route like: `routes.IgnoreRoute("{resource}.axd/{*pathInfo}");`
inside your `RouteConfig.cs`.


To use this module add the following elements inside the web.config
```
  <system.webServer>
   ...
    <modules>
	  ...
      <add name="WebMapOnlineUsersModule" type="UpgradeHelpers.WebMap.Server.OnlineUsersModule" />
    </modules>
    <handlers>
	   ...
      <add verb="*" path="sessioninfo.axd/sessionsinfo" name="SessionInfo" type="UpgradeHelpers.WebMap.Server.SessionsInfoHandler, OnlineUserModule" />
	  ...
	</handlers>

	...
   </system.webServer>
```
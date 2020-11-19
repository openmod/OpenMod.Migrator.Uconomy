
# UconomyToOpenMod
 Support rocketmod plugins to use OpenMod Economy

## Information
This plugin redirects Uconomy plugin requests to any economy plugin created for Openmod

## Warning
Some rocketmod plugins are imcompatible with last version of this plugin (ZaupShop for example)
if you need this to work with these plugins please check ***Instalation V2***

### Instalation
(V1)
You need to run `openmod install UconomyToOpenMod` on console to install the plugin.
And you are ready to go!

(V2)
You need to run `openmod install UconomyToOpenMod@1.0.4` on console to install the plugin.
You will also need to have Uconomy installed with rocketmod bridge.
And know you are ready to go with full compatibility!

### Data Migration
It is now possible to migrate uconomy data to openmod.
Just run this command: `MigrateUconomy [DeleteAfterMigrate]` (Console only)
'DeleteAfterMigrate' (true/false) is optional, this parameter deletes the old uconomy database after migrating it.

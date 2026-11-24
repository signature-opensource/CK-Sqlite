The model of the SQLite databases set up by the three-steps setup: the base real objects (`SqliteDatabase`, `SqliteDefaultDatabase`, `SqlitePackage`, `SqliteTable`) and their attributes (`SqlitePackageAttribute`, `SqliteTableAttribute`).

`SqliteAspectConfiguration` configures the setup aspect (its implementation is CK.Sqlite.Engine), and `TemporarySqliteDatabase` is a database file deleted when disposed.

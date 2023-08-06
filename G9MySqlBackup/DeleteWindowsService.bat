@echo off
SC STOP [G9TM][MySqlBackup]
SC DELETE [G9TM][MySqlBackup]

TIMEOUT /T 9 /NOBREAK
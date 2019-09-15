#!/bin/bash
xed /h/home/martin/Documents/GitHub/MonthlyBudgets/MonthlyBudgets/bin/Release/GameData/MonthlyBudgets/Changelog.cfg
xed /home/martin/Documents/GitHub/MonthlyBudgets/MonthlyBudgets/bin/Release/GameData/MonthlyBudgets/MonthlyBudgets.version
cp /home/martin/Documents/GitHub/MonthlyBudgets/MonthlyBudgets/bin/Release/MonthlyBudgets.dll /home/martin/Documents/GitHub/MonthlyBudgets/MonthlyBudgets/bin/Release/GameData/MonthlyBudgets
zip -r MonthlyBudgets.zip GameData
notify-send "Monthly Budgets build has finised"

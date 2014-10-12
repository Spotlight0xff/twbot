#!/usr/bin/python3

import json
import os
import math
import statistics


class col:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'

files = os.listdir("monitor")
print("Which village do you want to track?")

for fil in files:
    print("* " + fil)

print("\nID: ");
res = input();
file = open("monitor/monitor_"+res, 'r')
lastbuildings = ''

while True:
    line = file.readline();
    if line == "":
        break
    time = float(line.rstrip("\n")) / 1000 / 60;
    time = str(round(time, 2))
    line = file.readline();
    if line == "":
        break

    j = json.loads(line);
    if j['buildings'] != lastbuildings:
        lastbuildings = j['buildings']
    else:
        continue

    
    build = j['buildings']
    res = j['res']['resources']
    stor = j['res']['storage_max']
    wood = res['wood']
    stone = res['stone']
    iron = res['iron']
    mean = statistics.mean([wood, stone, iron])
    wood_col = col.ENDC
    stone_col = col.ENDC
    iron_col = col.ENDC
    
    if wood >= stor:
        wood_col = col.FAIL
    if abs(mean - wood) > mean/3:
        wood_col = col.WARNING

    if stone >= stor:
        stone_col = col.FAIL
    if abs(mean - stone) > mean/3:
        stone_col = col.WARNING

    if iron >= stor:
        iron_col = col.FAIL
    if abs(mean - iron) > mean/3:
        iron_col = col.WARNING
    end = col.ENDC


    print(time.ljust(20)+" | " + str(build['level']).ljust(2) + " | " + wood_col + str(wood).ljust(7) + end + " | " + stone_col + str(stone).ljust(7) + end + " | " + iron_col + str(iron).ljust(7) + end + " | " + str(stor).ljust(7));




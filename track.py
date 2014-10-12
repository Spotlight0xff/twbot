#!/usr/bin/python3

import json
import os

files = os.listdir("monitor")
print("Which village do you want to track?")

for fil in files:
    print("* " + fil)

print("\nID: ");
res = input();
file = open("monitor/monitor_"+res, 'r')

while True:
    line = file.readline();
    if line == "":
        break
    time = line.rstrip("\n");
    line = file.readline();
    if line == "":
        break
    j = json.loads(line);
    build = j['buildings']
    res = j['res']['resources']
    stor = j['res']['storage_max']

    print(time.ljust(20)+" | " + str(build['level']).ljust(2) + " | " + str(res['wood']).ljust(7) + " | " + str(res['stone']).ljust(7) + " | " + str(res['iron']).ljust(7) + " | " + str(stor).ljust(7));




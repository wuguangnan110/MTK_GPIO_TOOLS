@echo off
adb root
adb shell "echo 1 > /sys/class/misc/scp/scp_mobile_log"
adb shell "while true; do cat /dev/scp;done"
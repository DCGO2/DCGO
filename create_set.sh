#!/bin/bash
#Prerequisites: need to download github cli and will need bash
#On windows `Git for Windows` from git-scm comes with bash
#Must be logged in with gh auth login
#./create_set.sh ST25 15
#./create_set.sh BT25 104
set_name=$1
cards_in_set=$2

if [ $cards_in_set -gt 15 ]; then
	padding="%03g"
else
	padding="%02g"
fi

for i in `seq -f "$padding" 1 $cards_in_set`; do
	echo "Creating $set_name-$i: Not revealed"
	gh issue create -t "$set_name-$i: Not revealed" -l "New Card/Feature,Not Revealed" -b ""
done
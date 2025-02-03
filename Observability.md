#Splunk

##count occurrences of a specific string pattern in a JSON log entry over a given time period
index=<your_index>
| spath input=<json_field> output=<extracted_field>
| search <extracted_field>="<pattern>"
| timechart span=1h count by <extracted_field>

e.g.

index=app_logs
| spath input=log_message output=message
| search message="error"
| timechart span=1h count











#Datadog

#Splunk

# Splunk Query for Counting Occurrences of a Pattern in JSON Logs

This query extracts a field from a JSON log entry and counts occurrences of a specific string pattern over a period.

```splunk
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

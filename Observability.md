#Splunk

index=<your_index>
| spath input=<json_field> output=<extracted_field>
| search <extracted_field>="<pattern>"
| timechart span=1h count by <extracted_field>











#Datadog

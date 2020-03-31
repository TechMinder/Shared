$stsUrl = "<Token Provider URL>" 
$body = @{    
    client_id = 'ClientId'
    scope = '<Scope>'
	grant_type = '<implicit/password>'
}

$token = Invoke-RestMethod -Uri $stsUrl -Method Post -UseDefaultCredentials -Body $body



$headers = @{
	'Authorization' = 'bearer ' + $token.access_token
	}

#call another API and use the token obtain before

$apiUrl= '<api url>'
$result = Invoke-RestMethod -Uri $apiUrl -Method Get -Headers $headers

$resultjson = $result | ConvertTo-Json

 $actionMode = $resultjson | ConvertFrom-Json
 $releaseAction = $actionMode | where { $_.<PropertyName> -eq "<property value>" }

 
Write-Output "Access_Token" + $token.access_token
#Write-Output $actionMode
#Write-Output $releaseAction

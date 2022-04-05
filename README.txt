- Change the properies of the solution to use "Multiple startup projects". Set all 3 projects to start.
- Start debugging. Make sure all 3 servers (client, server and external IdP) are running.
- Go to the client (https://localhost:7216) and try two send requests to the autorize endpoint (GET and POST). 
=> the current state: we get an access token with GET request, but the POST request does not work.

External IdP user:

user: mei
pwd: test

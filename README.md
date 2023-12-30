# WebBots-API
Deployment ready API for efficient communication with NLU bots. 

_I wrote this API intially for internal use at Araka Systems. I have taken permission from the team at Araka Systems to make it public on my own github for the purpose of showing proof of work for future opportunities._

This project is built with ASP.NET Core 6.

To deploy this service on GCP Container Registry
docker build -t gcr.io/<project-name>/<service-name> .
gcloud auth login
gcloud auth configure-docker
docker push gcr.io/my-project/my-image:tag1

For any queries, email me at bilalshafimirza@gmail.com

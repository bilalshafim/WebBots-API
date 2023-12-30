# Araka-WebBots
 Araka System's internal API for communication with Conversational AI bots.

## Service is running on Google Cloud Run

### To deploy this service on GCP Container Registry

##### docker build -t gcr.io/chromatic-craft-384800/web-bots .
##### gcloud auth login
##### gcloud auth configure-docker
##### docker push gcr.io/my-project/my-image:tag1
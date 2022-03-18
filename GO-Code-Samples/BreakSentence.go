package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"net/url"
	"os"
)

func main() {
	/*
	 * Read your resource key from an env variable.
	 * Please note: You can replace this code block with
	 * var resourceKey = "YOUR_RESOURCE_KEY" if you don't
	 * want to use env variables. If so, be sure to delete the "os" import.
	 */
	if "" == os.Getenv("TRANSLATOR_TEXT_RESOURCE_KEY") {
		log.Fatal("Please set/export the environment variable TRANSLATOR_TEXT_RESOURCE_KEY.")
	}
	resourceKey := os.Getenv("TRANSLATOR_TEXT_RESOURCE_KEY")

	if "" == os.Getenv("TRANSLATOR_TEXT_REGION") {
		log.Fatal("Please set/export the environment variable TRANSLATOR_TEXT_REGION.")
	}
	region := os.Getenv("TRANSLATOR_TEXT_REGION")

	if "" == os.Getenv("TRANSLATOR_TEXT_ENDPOINT") {
		log.Fatal("Please set/export the environment variable TRANSLATOR_TEXT_ENDPOINT.")
	}
	endpoint := os.Getenv("TRANSLATOR_TEXT_ENDPOINT")
	uri := endpoint + "/breaksentence?api-version=3.0"
	/*
	 * This calls our breakSentence function, which we'll
	 * create in the next section. It takes a single argument,
	 * the resource key.
	 */
	breakSentence(resourceKey, region, uri)
}

func breakSentence(resourceKey string, region string, uri string) {
	// Build the request URL. See: https://golang.org/pkg/net/url/#example_URL_Parse
	u, _ := url.Parse(uri)
	q := u.Query()
	q.Add("languages", "en")
	u.RawQuery = q.Encode()

	// Create an anonymous struct for your request body and encode it to JSON
	body := []struct {
		Text string
	}{
		{Text: "How are you? I am fine. What did you do today?"},
	}
	b, _ := json.Marshal(body)

	// Build the HTTP POST request
	req, err := http.NewRequest("POST", u.String(), bytes.NewBuffer(b))
	if err != nil {
		log.Fatal(err)
	}
	// Add required headers to the request
	req.Header.Add("Ocp-Apim-Subscription-Key", resourceKey)
	req.Header.Add("Ocp-Apim-Subscription-Region", region)
	req.Header.Add("Content-Type", "application/json")

	// Call the Translator Text API
	res, err := http.DefaultClient.Do(req)
	if err != nil {
		log.Fatal(err)
	}

	// Decode the JSON response
	var result interface{}
	if err := json.NewDecoder(res.Body).Decode(&result); err != nil {
		log.Fatal(err)
	}
	// Format and print the response to terminal
	prettyJSON, _ := json.MarshalIndent(result, "", "  ")
	fmt.Printf("%s\n", prettyJSON)
}

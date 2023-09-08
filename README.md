# RinhaDeBackend

This project was my participation in [Rinha de Backend](https://github.com/zanfranceschi/rinha-de-backend-2023-q3). The central idea was to build the most performatic API while adhering to some rules and run a stress test to determine which one performs better.

The API has 3 endpoints:

-   **POST**: /pessoas
    -   Performs body validations and creates the user in the database.
-   **GET**: /pessoas/id
    -   Searches for the person with that ID in the database.
-   **GET**: /pessoas?t=jo
    -   Searches for all users whose names, nicknames, or stacks contain "jo".

I made two completely different implementations:
- Usign Redis as a cache database:
	- Code available on "[redis-implementation](https://github.com/GuilhermeSAraujo/rinha-backend/tree/redis-implementation)" branch.
- Using pub/sub design pattern, with Nats server:
	- Code available on "[pub/sub-implementation](https://github.com/GuilhermeSAraujo/rinha-backend/tree/pub/sub-implementation)" branch.

## Comparison

> Redis implementation always on the left, pub/sub on the right.

### Inserts
The principal metric of the challenge was the user inserts into the database. Pub/sub implementation performed better in this aspect.

Average inserts:
| Redis | Pub/Sub |
|--|--|
| 38880 | 40400 |

###  Responses success rate

| Redis | Pub/Sub |
|--|--|
| ![redis](./assets/redis-imp-succs-responses.png) | ![pub/sub](./assets/pub-sub-imp-succs-responses.png) |

### Response time ranges
| Redis | Pub/Sub |
|--|--|
| ![redis](./assets/redis-imp-res-time-range.png) | ![pub/sub](./assets/pub-sub-imp-res-time-range.png) |

## Conclusion
Looking at the numbers, it's clear that the Publisher/Subscriber implementation had better metrics in all aspects. However, it's possible to argue against this when considering real-life scenarios.

The primary reason to question the pub/sub implementation is the asynchronous database inserts. Since the challenge requires the API to return a 2XX status code, it generates a fake success response because the request is queued for insertion into the database. If an error occurs during the insertion, the user won't be able to know.

So, for a real-life scenario, it's important to persist the data and provide a genuine success response to the user. In the pub/sub implementation, the correct response would be a 301 status code, indicating that the insertion was successfully completed.

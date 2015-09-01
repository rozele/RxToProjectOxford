# RxToProjectOxford
Reactive Extensions for Project Oxford

## Dependencies
* [Rx.NET](https://github.com/Reactive-Extensions/Rx.NET)

## About

The speech recognition module in the [Project Oxford](http://projectoxford.ai) SDKs supports partial recognition feedback while converting user utterances to text. Generally, supporting this kind of feedback in the UI reduces the perceived latency of the speech recognition operation.

Modeling partial feedback and final responses, along with any speech recognition errors, using the reactive programming model creates an explicit semantics for the API that is not provided by the .NET event-driven API in the Project Oxford SDK.

This repository provides a basic implementation for subscribing to the Project Oxford client events and converting these events into a higher-order observable, i.e., IObservable<IObservable<T>>, where each inner observable is a single sequence of partial results terminated by a final result.

For a more detailed summary, check out [this blog post](https://ericroz.wordpress.com/2015/08/31/reactive-extensions-for-project-oxford-speech-to-text/).

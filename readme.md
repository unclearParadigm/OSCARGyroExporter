# OSCAR Gyroscope Exporter

is an exporter to create an [OSCAR](https://www.sleepfiles.com/OSCAR/)-compatible CSV-file (that can be imported to OSCAR) from recordings of [Physics Toolbox Sensor Suite](https://play.google.com/store/apps/details?id=com.chrystianvieyra.physicstoolboxsuite&hl=en) Android App.

## Physics Toolbox Sensor Suite

is an Android-App capable of capturing Gyroscop-Sensor-Data in CSV-format.

## OSCAR (the Open Source CPAP Analysis Reporter)

is a Desktop-application capable of displaying various sleep metrics (including CPAP, pulsoxymetric measurments) and the sleeping-position.

## What does this application do?

it exports the captured gyroscope data from `Physics Toolbox Sesnor Suite`-app
to a format that `OSCAR` can import.

## Who needs this?

people monitoring their sleep, mostly due to medical conditions. The sleeping position is heavily impacting how we sleep and the overall quality of sleep.
For optimizing the quality and duration of your sleep, this App helps you better
analyze your sleep - completly free of charge.

## Features

* Read from an export of the Android-app
* Create a OSCAR-compatible output-CSV
* Console Application (slick, simple, automatable)
* Possibility to enter reference date
* Language in German
* Cross-Platform (runs on Linux, Windows, MacOS)

## How to use

Next to the executable (*.exe), put the input file which must be called "in.csv".
Start the application. Enter the reference date (in format 'YYYY-MM-DD'), wait for
the application to finish. Press any key to close the application. Inside the same directory the `in.csv` file was put, there is now a `out.csv` file which can now be imported into OSCAR.


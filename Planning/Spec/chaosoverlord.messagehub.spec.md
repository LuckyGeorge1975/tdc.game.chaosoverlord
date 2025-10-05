# Chaos Overlords MessageHub Specification

## Aufgabe des MessageHubs

Der MessageHub dient als zentraler Kommunikationsmechanismus im Chaos Overlords Projekt, um lose Kopplung zwischen Klassen zu gewährleisten. Anstatt direkte Eventhandler oder Methodenaufrufe zwischen abhängigen Klassen zu verwenden, werden Messages über den Hub publisht und von interessierten Subscribern empfangen. Dies reduziert Abhängigkeiten, verbessert die Testbarkeit und ermöglicht eine flexiblere Architektur.

### Arbeitsweise

- **Publisher**: Klassen, die Messages senden, indem sie `_messageHub.Publish(message)` aufrufen.
- **Subscriber**: Klassen registrieren sich mit `_messageHub.Subscribe<TMessage>(handler)`, um Messages zu empfangen.
- **Messages**: Sind einfache Records oder Klassen, die die Daten kapseln.
- **Lebenszyklus**: Subscriber müssen `IDisposable` implementieren und Subscriptions disposen, um Memory Leaks zu vermeiden.
- **Threading**: Der Hub ist thread-safe und verwendet WeakReferences für Subscriptions.

### Regeln

- Keine direkten Eventhandler zwischen ViewModels oder Services (außer für interne Events wie PropertyChanged oder CollectionChanged in MVVM).
- Commands für UI-Interaktionen.
- Messages für asynchrone, lose gekoppelte Kommunikation.

## Tabelle der Messages

| Message | Publisher | Subscriber | Beschreibung |
|---------|-----------|------------|--------------|
| TurnSummaryChangedMessage | TurnViewModel | MapViewModel, TurnManagementSectionViewModel | Wird gesendet, wenn sich Turn-Phase, -Nummer oder -Status ändert. |
| CommandTimelineUpdatedMessage | TurnViewModel | - | Wird gesendet, wenn die Command-Timeline aktualisiert wird. |
| TurnEventsChangedMessage | TurnViewModel | EventFeedViewModel | Wird gesendet, wenn Events im EventLog hinzugefügt oder entfernt werden. |
package main

import (
    "fyne.io/fyne"
    "fyne.io/fyne/widget"
    "fyne.io/fyne/app"
    "fyne.io/fyne/layout"
)

func main() {
    app := app.New()

    w := app.NewWindow("ModdingTales Socket API GUI")
    w.SetContent(
        fyne.NewContainerWithLayout(layout.NewGridLayout(2),
            widget.NewVBox(
                widget.NewLabel("Hello Fyne!"),
                widget.NewLabel("Hello Fyne!"),
            ),
            widget.NewButton("Quit", func() {
                app.Quit()
            }),
        ),
    );
    // w.SetContent(widget.NewVBox(
    //     widget.NewLabel("Hello Fyne!"),
    //     widget.NewButton("Quit", func() {
    //         app.Quit()
    //     }),
    // ))

    w.ShowAndRun()
}
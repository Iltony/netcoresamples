import {Component} from "@angular/core";
@Component({
selector: "home",
template: `
<h2>{{title}}</h2>
<item-list class="latest"></item-list>
<item-list class="most-viewed"></item-list>
<item-list class="random"></item-list>
`,
styles: []
})
export class HomeComponent {
title = "Welcome View";
}
import { Component, Input, OnInit } from "@angular/core";
import { Item } from "./item";
import { ItemService } from "./item.service";
import { Router } from "@angular/router";

@Component({
    selector: "item-list",
    template: `
        <h3>{{title}}</h3>
        <ul class="items">
            <li *ngFor="let item of items" [class.selected]="item === selectedItem" (click)="onSelect(item)">
                <div class="title">{{item.Title}}</div>
                <div class="description">{{item.Description}}</div>
            </li>
        </ul>
`    
})

export class ItemListComponent implements OnInit {
    @Input() class: string;
    title: string;
    selectedItem: Item;
    items: Item[];
    errorMessage: string;

    constructor(private itemService: ItemService, private router: Router) { }

    ngOnInit() {
        console.log("ItemListComponent instantiated with the following type: " + this.class);
        var s = null;
        switch (this.class) {
            case "latest":
            default:
                this.title = "Latest Items";
                s = this.itemService.getLatest();
                break;
            case "most-viewed":
                this.title = "Most Viewed Items";
                s = this.itemService.getMostViewed();
                break;
            case "random":
                this.title = "Random Items";
                s = this.itemService.getRandom();
                break;
        }
        s.subscribe(
            items => this.items = items,
            error => this.errorMessage = <any>error
        );
    }

    getLatest() {
        this.itemService.getLatest()
            .subscribe(
            latestItems => this.items = latestItems,
            error => this.errorMessage = <any>error
            );
    }

    onSelect(item: Item) {
        this.selectedItem = item;
        console.log("Item " + this.selectedItem.Id + " has been clicked: loading item viewer...");
        this.router.navigate(["item/view", this.selectedItem.Id]);
    }
}
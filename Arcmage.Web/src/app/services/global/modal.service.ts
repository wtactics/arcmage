import { Subject, Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { ModalViewModel } from 'src/app/models/view-models/modal-viewmodel';

@Injectable()
export class ModalService<T> {
    private modalShown: Subject<ModalViewModel<T>> = new Subject<ModalViewModel<T>>();
    public modalShown$: Observable<ModalViewModel<T>> = this.modalShown.asObservable();

    private modalHidden: Subject<boolean> = new Subject<boolean>();
    public modalHidden$: Observable<boolean> = this.modalHidden.asObservable();

    private modalSaved: Subject<ModalViewModel<T>> = new Subject<ModalViewModel<T>>();
    public modalSaved$: Observable<ModalViewModel<T>> = this.modalSaved.asObservable();
    
    openModal(model: ModalViewModel<T>){
        this.modalShown.next(model);
    }

    closeModal(){
        this.modalHidden.next();
    }

    saveModal(model: ModalViewModel<T>){
        this.modalSaved.next(model);
    }
}
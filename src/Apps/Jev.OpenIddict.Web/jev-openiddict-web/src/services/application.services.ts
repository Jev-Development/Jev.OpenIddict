import axios, { AxiosError, AxiosInstance, AxiosResponse } from 'axios';
import { AnyAction, Dispatch } from 'redux';
import { AlertActions } from '../actions/alert-state.actions';
import { ApplicationViewDto } from '../dto/application-view.dto';
import { ErrorResponseModel } from '../dto/error-response.dto';
import { ApplicationStore } from '../store';

class ApplicationService {
  private _axios: AxiosInstance;

  constructor(private _dispatch: Dispatch<AnyAction>) {
    this._axios = axios.create({
      baseURL: `${window.location.origin}/api/v1/application`
    });
  }

  public getByClientId(clientId: string): Promise<ApplicationViewDto> {
    return new Promise<ApplicationViewDto>((resolve: (str: ApplicationViewDto) => void, reject: (error: any) => void) => {
      this._axios
        .get<ApplicationViewDto>(clientId)
        .then((response: AxiosResponse) => {
          resolve(response.data);
        })
        .catch((error: AxiosError<ErrorResponseModel>) => {
          this._dispatch(
            AlertActions.addToQueue({
              header: `Server error response: ${error.response?.data?.responseCode ?? '500'}`,
              message: error.response?.data?.message ?? 'Unkown error from server',
              type: 'danger'
            })
          );
          reject(error);
        });
    });
  }
}

export const ApplicationApiService: ApplicationService = new ApplicationService(ApplicationStore.dispatch);
